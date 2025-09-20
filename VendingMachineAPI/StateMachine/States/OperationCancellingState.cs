using VendingMachineAPI.Models;
using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine.States;

public class OperationCancellingState : IVendingState
{
    public async Task OnOkAsync(IVendingContext context)
    {
        var balance = await context.MonyService.GetBalance();
        string display;
        if (balance > 0)
        {
            await context.MonyService.RefundAsync(0);
            display = $"Refunded {balance:C}. Back to ready.";
        }
        else
        {
            display = "No balance to refund. Back to ready.";
        }

        // Reset state
        context.Selection = null;
        
        await context.NotifyAsync(display);
        await context.TransitionToAsync(new ReadyState());
    }

    public Task OnCancelAsync(IVendingContext context)
    {
        // User pressed Cancel again → abort cancellation, back to ready
        string display = "Please press ok to confirm the cacelation.";
        context.Selection = null;
        return context.NotifyAsync(display);
    }

    public Task OnKeyPressAsync(IVendingContext context, string key)
    {
        // Ignore key presses in cancellation mode
        string display = "Cancelling... Please confirm with OK.";
        return context.NotifyAsync(display);
    }
}

