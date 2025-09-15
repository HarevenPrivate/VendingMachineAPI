namespace VendingMachineAPI.Interface
{
    public interface IKeyboardService
    {
        ValueTask EnqueueKeyPressAsync(string key);
        ValueTask EnqueueOkAsync();
        ValueTask EnqueueCancelAsync();
    }
}
