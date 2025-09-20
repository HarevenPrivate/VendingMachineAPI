using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine.States;

public class ProductNotAvailableState : IVendingState
{
    public Task OnKeyPressAsync(IVendingContext context, string key)
    {
        // Ignore input while product is not available
        string sisplay = "Invalid product. Please press Cancel.";
        return context.NotifyAsync(sisplay);
    }

    public Task OnOkAsync(IVendingContext context)
    {
        // Ignore OK, force Cancel
        string display = "Invalid product. Please press Cancel.";
        return context.NotifyAsync(display);
    }

    public async Task OnCancelAsync(IVendingContext context)
    {
        var balance = await context.MonyService.GetBalance();
        string? display;
        if (balance > 0)
        {
            await context.MonyService.RefundAsync(0);
            display = $"Refunding {balance:C}. System ready.";
        }
        else
        {
            display = "Cancelled. System ready.";
        }

        // Reset state
        context.Selection = null;



        await context.NotifyAsync(display);

        // Transition back to ReadyState
        await context.TransitionToAsync(new ReadyState());
    }
}
