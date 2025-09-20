using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine.States;

public class SystemNotWorkingState : IVendingState
{
    public Task OnKeyPressAsync(IVendingContext context, string key)
        => context.NotifyAsync("System offline. Please wait...");

    public Task OnOkAsync(IVendingContext context)
        => context.NotifyAsync("System offline. Please wait...");

    public Task OnCancelAsync(IVendingContext context)
        => context.NotifyAsync("System offline. Please wait...");

    public Task OnEnterAsync(IVendingContext context)
        => context.NotifyAsync("System is offline due to thermostat error.");

    public Task OnExitAsync(IVendingContext context)
        => context.NotifyAsync("System back online.");
}

