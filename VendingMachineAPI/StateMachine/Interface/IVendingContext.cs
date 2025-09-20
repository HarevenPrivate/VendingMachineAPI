using VendingMachineAPI.Interface;

namespace VendingMachineAPI.StateMachine.Interface;

public interface IVendingContext
{
    // Transitions between states
    void TransitionTo(IVendingState newState);

    // Accessors to machine metadata or services
    Task NotifyAsync(string message);

    // For product/price lookup, money, etc.
    bool IsThermostatWorking();
    IMoneyDeviceService MonyService { get; }
    IProductRepository Productes { get; }

    // Expose the current selection if needed
    string? Selection { get; set; }

    Task OnKeyPressAsync(string key);
    Task OnOkAsync();
    Task OnCancelAsync();
}

