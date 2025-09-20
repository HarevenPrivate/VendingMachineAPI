using VendingMachineAPI.Interface;
using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine;

public class VendingContext(
    IVendingState initialState,
    IPanelService panel,
    IProductRepository products,
    IMoneyDeviceService money,
    IThermostatService thermostat) : IVendingContext
{
    private IVendingState _state = initialState;
    private readonly IPanelService _panel = panel;
    private readonly IProductRepository _products = products;
    private readonly IMoneyDeviceService _money = money;
    private readonly IThermostatService _thermostat = thermostat;

    public string? Selection { get; set; }

    public async void TransitionTo(IVendingState newState)
    {
        if (_state != null)
            await _state.OnExitAsync(this);

        _state = newState;

        if (_state != null)
            await _state.OnEnterAsync(this);
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

