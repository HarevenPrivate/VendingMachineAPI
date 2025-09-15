namespace VendingMachineAPI.Interface
{
    public interface IMoneyDeviceService
    {
        event Action<decimal> BalanceChanged;
        Task InsetMoneyAsync(decimal amount);

        decimal GetBalance();
        Task RefundAsync(decimal price);
    }
}
