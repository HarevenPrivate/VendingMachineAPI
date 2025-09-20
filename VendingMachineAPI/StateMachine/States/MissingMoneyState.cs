using VendingMachineAPI.Models;
using VendingMachineAPI.Services;
using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine.States;

public class MissingMoneyState : IVendingState
{
    public Task OnKeyPressAsync(IVendingContext context, string key)
    {
        // Ignore new key presses, still waiting for money
        return context.NotifyAsync("Please insert sufficient money, or press Cancel.");
    }

    public async Task OnEnterAsync(IVendingContext context)
    {
        if (context.MonyService != null)
            context.MonyService.BalanceChanged += amount => OnBalanceChanged(context, amount);
        await Task.CompletedTask;
    }

    public async Task OnExitAsync(IVendingContext context)
    {
        if (context.MonyService != null)
            context.MonyService.BalanceChanged -= amount => OnBalanceChanged(context, amount);
        await Task.CompletedTask;
    }

    private static async Task OnBalanceChanged(IVendingContext context, decimal amount)
    {
        if( context.Selection == null || context.Selection.Length != 2)
        {
            // No selection, should not be here
            await context.TransitionToAsync(new ReadyState());
            return;
        }
        string display;
        var (price, productName) = context.Productes.GetProduct(context.Selection);
        if (amount >= price)
        {
            decimal refund = amount - price;
            display = $"Please press OK to take the {productName} and refund {refund:C}" ;
        }
        else
        {
            decimal missing = price - amount;
            display = $"{productName} cost {price:C}, Missing {missing:C}";
        }

        await context.NotifyAsync(display);
    }

    public async Task OnOkAsync(IVendingContext context)
    {
        var (price, productName) = context.Productes.GetProduct(context.Selection!);
        var balance = await context.MonyService.GetBalance();

        if (balance >= price)
        {
            // Dispense product
            decimal change = balance - price;

            await context.MonyService.RefundAsync(price);

            string display = change > 0
                ? $"Dispensing {productName}, returning {change:C}"
                : $"Dispensing {productName}";

            // Reset
            context.Selection = null;
            await context.NotifyAsync(display);
            await context.TransitionToAsync(new ReadyState());
        }
        else
        {
            decimal missing = price - balance;
            string display = $"{productName} costs {price:C}, still missing {missing:C}";
            await context.NotifyAsync(display);
        }
    }

    public async Task OnCancelAsync(IVendingContext context)
    {
        var balance = await context.MonyService.GetBalance();
        
        string display = balance > 0
            ? $"Cancel requested. Press OK to refund {balance:C}."
            : "Cancel requested. Press OK to finish.";

        await context.NotifyAsync(display);

        // Transition into cancellation flow
        await context.TransitionToAsync(new OperationCancellingState());
    }
}


