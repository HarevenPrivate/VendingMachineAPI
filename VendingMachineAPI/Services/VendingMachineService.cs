using System.Threading.Channels;
using VendingMachineAPI.Models;
using VendingMachineAPI.Interface;


namespace VendingMachineAPI.Services;

public sealed class VendingMachineService : IVendingMachineService, IAsyncDisposable
{
    private readonly Channel<VendingEvent> _channel = Channel.CreateUnbounded<VendingEvent>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processorTask;
    private readonly IPanelService _panelNotifier;
    private readonly IProductRepository _productRepository;
    private readonly IMoneyDeviceService _moneyDeviceService;
    private readonly IThermostatService _thermostatService;
    private readonly SemaphoreSlim _lock = new(1,1);

    // The machine state (single-authority)
    private readonly VendingState _state = new();

    // Simple product catalog (two-letter code -> price)
   

    public VendingMachineService(IPanelService panelNotifier, IProductRepository productRepository, IMoneyDeviceService moneyDeviceService, IThermostatService thermostatService)
    {
        _thermostatService = thermostatService;
        _moneyDeviceService = moneyDeviceService;
        _productRepository = productRepository;
        _panelNotifier = panelNotifier;
        _processorTask = Task.Run(ProcessEventsAsync);
        _moneyDeviceService.BalanceChanged += OnBalanceChanged;
    }

    private async Task OnBalanceChanged(decimal amount)
    {
        await _lock.WaitAsync();
        try
        {
            if (_state.Status == VendingStateSatus.ProductSeletedMissingMoey)
            {
                if (_state.Selection != null)
                {

                    (decimal price, string productName) = _productRepository.GetProduct(_state.Selection);
                    if (amount > price)
                    {
                        _state.Display = $"Please press ok to take the {productName}";
                        _state.BlockInputs = false;
                        _state.Status = VendingStateSatus.Ready;
                    }
                    else
                    {
                        decimal missing = price - _moneyDeviceService.GetBalance();
                        _state.Display = $"Please press ok to take the {productName}";
                        _state.BlockInputs = true;
                        _state.Display = $"{productName} cost {price:C}, Missing {missing:C}";

                    }
                    await NotifyPanelAsync(_state.Display);
                }

            }
        }
        finally
        {             
            _lock.Release();
        }
    }

    #region Public API - enqueue events

    public ValueTask EnqueueKeyPressAsync(string key) => _channel.Writer.WriteAsync(new KeyPressEvent(key));
    public ValueTask EnqueueOkAsync() => _channel.Writer.WriteAsync(new OkEvent());
    public ValueTask EnqueueCancelAsync() => _channel.Writer.WriteAsync(new CancelEvent());

    public VendingState Snapshot()
    {
        // Return a shallow copy for controllers to read
        lock (_state)
        {
            return _state.Clone();
        }
    }
    #endregion

    private async Task ProcessEventsAsync()
    {
        await foreach (var ev in _channel.Reader.ReadAllAsync(_cts.Token))
        {
            try
            {
                // All event handling is done synchronously on this single consumer.
                switch (ev)
                {
                    case KeyPressEvent kp:
                        HandleKeyPress(kp.Key);
                        break;
                    case OkEvent:
                        await HandleOkAsync();
                        break;
                    case CancelEvent:
                        await HandleCancelAsync();
                        break;
                    default:
                        await NotifyPanelAsync("Unknown event");
                        break;
                }
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                await NotifyPanelAsync($"Internal error: {ex.Message}");
            }
        }
    }

    #region Handlers (single-threaded)

    private void HandleKeyPress(string key)
    {
        // Keys are letters or digits; special keys are "OK" and "CANCEL" and handled elsewhere.
        lock (_state)
        {
            if (!_thermostatService.Isworking())
            {
                _state.Display = "Thermostat error - machine offline";
                _ = NotifyPanelAsync(_state.Display);
                return;
            }

            if (_state.BlockInputs)
            {
                // Optionally ignore
                _state.Display = "Machine busy";
                _ = NotifyPanelAsync(_state.Display);
                return;
            }

            // Build selection: allow letters and digits only; selection resets on new transaction boundary
            _state.Selection ??= string.Empty;

            // limit selection length to 2 (letter + digit)
            if (_state.Selection.Length >= 2)
            {
                // New press restarts selection
                _state.Selection = key;
            }
            else
            {
                _state.Selection += key;
            }

            _state.Display = $"Selected: {_state.Selection}";
        }

        _ = NotifyPanelAsync(_state.Display);
    }

    private async Task HandleOkAsync()
    {
        string? selection;
        

        lock (_state)
        {
            if (!_thermostatService.Isworking())
            {
                _state.Display = "Thermostat error - cannot proceed";
                _ = NotifyPanelAsync(_state.Display);
                return;
            }

            if(_state.Status == VendingStateSatus.OperationCancelling)
            {
                decimal refundAmount = _moneyDeviceService.GetBalance();
                _state.Inserted = 0;
                _state.Status = VendingStateSatus.Ready;
                _state.Selection = null;
                _state.BlockInputs = false;
                _state.Display = refundAmount > 0
                    ? $"Refunding {refundAmount:C} System ready"
                    : "System ready";

                if (refundAmount > 0)
                {
                    _moneyDeviceService.RefundAsync(0);
                }
                _ = NotifyPanelAsync(_state.Display);
                return;
            }

            selection = _state.Selection;

            // If no selection, just inform user
            if (string.IsNullOrWhiteSpace(selection))
            {
                _state.Display = "No selection";
                _ = NotifyPanelAsync(_state.Display);
                return;
            }

            // Block inputs while we decide/dispense so we don't race with other key events
            _state.BlockInputs = true;
        }

        // Validate product
        if (selection == null || !_productRepository.IsExist(selection))
        {
            // Invalid product -> refund any money
            decimal refundAmount;
            lock (_state)
            {
                refundAmount = _moneyDeviceService.GetBalance();
                _state.Inserted = _moneyDeviceService.GetBalance();
                _state.Status = VendingStateSatus.ProductNotAvailable;
                _state.Selection = null;
                _state.BlockInputs = true;
                _state.Display = refundAmount > 0
                    ? $"Product not exist. Please Press Cancel to Refunding {refundAmount:C}"
                    : "Product not exist. Please Press Cancel";
            }

            

            await NotifyPanelAsync(_state.Display);
            return;
        }

        var (price, description) = _productRepository.GetProduct(selection);
        // valid product
        if (_moneyDeviceService.GetBalance() >= price)
        {
            decimal change = _moneyDeviceService.GetBalance() - price;

            lock (_state)
            {
                _state.Display = change > 0
                    ? $"Dispensing {selection} Name: {description}. Returning {change:C}"
                    : $"Dispensing {selection} Name: {description}";
            }


            lock (_state)
            {
                _state.Selection = null;
                _state.Inserted = 0;
                _state.BlockInputs = false;

                _moneyDeviceService.RefundAsync(price);
            }

            await NotifyPanelAsync(_state.Display);
            return;
        }
        else
        {
            decimal missing = price - _moneyDeviceService.GetBalance();
            lock (_state)
            {
                _state.Display = $"{description} cost {price:C}, Missing {missing:C}";
                _state.BlockInputs = true;
                _state.Status = VendingStateSatus.ProductSeletedMissingMoey;
            }

            await NotifyPanelAsync(_state.Display);
            return;
        }
    }

    private async Task HandleCancelAsync()
    {
        decimal refundAmount;
        lock (_state)
        {
            if(  _state.Status == VendingStateSatus.ProductNotAvailable)
            {
                refundAmount = _moneyDeviceService.GetBalance();
                _state.Inserted = 0;
                _state.Selection = null;
                _state.BlockInputs = false;
                _state.Status = VendingStateSatus.Ready;
                _state.Display = refundAmount > 0 ? $"Cancelled - Refunding {refundAmount:C}" : "Cancelled";
                _moneyDeviceService.RefundAsync(0);
            }
            else
            {
                refundAmount = _moneyDeviceService.GetBalance();
                _state.Inserted = _moneyDeviceService.GetBalance();
                _state.Selection = null;
                _state.BlockInputs = true;
                _state.Status = VendingStateSatus.OperationCancelling;
                _state.Display = refundAmount > 0 ? $"Cancelled - Press OK for Refunding {refundAmount:C}" : "Cancelled press OK to finish operation";
                
            }
            // If there is nothing to cancel, just display Cancelled

        }

        await NotifyPanelAsync(_state.Display);
    }

    

    #endregion

    #region Notifications (SignalR)

    private async Task NotifyPanelAsync(string message)
    {
        await _panelNotifier.NotifyPanelAsync(message, Snapshot());
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _cts.Cancel();
        try { await _processorTask.ConfigureAwait(false); } catch { /* ignored */ }
        _cts.Dispose();
    }
}
