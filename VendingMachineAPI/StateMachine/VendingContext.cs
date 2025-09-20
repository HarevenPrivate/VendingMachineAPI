using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;
using VendingMachineAPI.StateMachine.Interface;
using VendingMachineAPI.StateMachine.States;

namespace VendingMachineAPI.StateMachine;

public class VendingContext: IVendingContext
{
    private IVendingState _state;
    private readonly IPanelService _panel;
    private readonly IProductRepository _products;
    private readonly IMoneyDeviceService _money;
    private readonly IThermostatService _thermostat;
    private readonly SemaphoreSlim _lock = new(1, 1);
    //panel, products, money, thermostat
    public VendingContext(
        IVendingState initialState,
        IPanelService panelService,
        IProductRepository productRepository,
        IMoneyDeviceService moneyService,
        IThermostatService thermostatService
        )
    {
        _state = initialState;
        _money = moneyService;
        _products = productRepository;
        _thermostat = thermostatService;
        _panel = panelService;

        // 🔹 Register here, so the context supervises state transitions
        _thermostat.StatusChange += async working =>
        {
            if (!working)
            {
                await TransitionToAsync(new SystemNotWorkingState());
            }
            else if (_state is SystemNotWorkingState)
            {
                await TransitionToAsync(new ReadyState());
            }
        };
    }

    public string? Selection { get; set; }

    public async Task TransitionToAsync(IVendingState newState)
    {
        await _lock.WaitAsync();
        try {
            if (_state != null)
                await _state.OnExitAsync(this);

            _state = newState;

            if (_state != null)
                await _state.OnEnterAsync(this);
        }
        finally
        {
            _lock.Release();
        }
        
    }

    


public Task NotifyAsync(string message)
        => _panel.NotifyPanelAsync(message); // you can pass state snapshot if needed

    public bool IsThermostatWorking() => _thermostat.Isworking();

    public IMoneyDeviceService MonyService => _money;

    public IProductRepository Productes => _products;

    // Delegation to the state
    public Task OnKeyPressAsync(string key) => _state.OnKeyPressAsync(this, key);
    public Task OnOkAsync() => _state.OnOkAsync(this);
    public Task OnCancelAsync() => _state.OnCancelAsync(this);
}

