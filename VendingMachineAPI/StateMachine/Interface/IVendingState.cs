namespace VendingMachineAPI.StateMachine.Interface;

public interface IVendingState
{
    Task OnKeyPressAsync(IVendingContext context, string key);
    Task OnOkAsync(IVendingContext context);
    Task OnCancelAsync(IVendingContext context);

    Task OnEnterAsync(IVendingContext context) => Task.CompletedTask;
    Task OnExitAsync(IVendingContext context) => Task.CompletedTask;
}

