using VendingMachineAPI.Models;
using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.StateMachine.States;

public class ReadyState : IVendingState
{
    public async Task OnKeyPressAsync(IVendingContext context, string key)
    {
        // Ensure selection is initialized
        context.Selection ??= string.Empty;

        if (context.Selection.Length >= 2)
        {
            // Restart selection if already 2 chars
            context.Selection = key;
        }
        else
        {
            context.Selection += key;
        }

        string display = $"Selected: {context.Selection}";
        await context.NotifyAsync(display);
    }

    public async Task OnOkAsync(IVendingContext context)
    {
        var selection = context.Selection;

        if (string.IsNullOrWhiteSpace(selection) || selection.Length < 2)
        {
            await context.NotifyAsync("Please select a valid product");
            return;
        }

        if (!context.Productes.IsExist(selection))
        {
            await context.NotifyAsync("Product not available. Press Cancel.");
            // switch state
            await context.TransitionToAsync(new ProductNotAvailableState());
            return;
        }

        var (price, description) = context.Productes.GetProduct(selection);
        var balance = await context.MonyService.GetBalance();

        if (balance >= price)
        {
            decimal change = balance - price;
            string Display = change > 0
                ? $"Dispensing {description}. Returning {change:C}"
                : $"Dispensing {description}";

            await context.MonyService.RefundAsync(price);

            context.Selection = null;
            

            await context.NotifyAsync(Display);
            // remain in ReadyState
        }
        else
        {
            decimal missing = price - balance;
            
            string display = $"{description} cost {price:C}, Missing {missing:C}";
            

            await context.NotifyAsync(display);
            // switch to MissingMoneyState
            await context.TransitionToAsync(new MissingMoneyState());
        }
    }

    public async Task OnCancelAsync(IVendingContext context)
    {
        // Clear selection and reset
        context.Selection = null;
        
        string display = "Please press ok. To set the system ready ready";

        await context.NotifyAsync(display);

        await context.TransitionToAsync(new OperationCancellingState());
    }
}

