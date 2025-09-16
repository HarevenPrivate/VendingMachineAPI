namespace VendingMachineAPI.Interface
{
    public interface IMoneyDeviceService
    {
        event Func<decimal,Task>? BalanceChanged;
        Task InsetMoneyAsync(decimal amount);

        Task<decimal> GetBalance();
        Task RefundAsync(decimal price);
    }
}
