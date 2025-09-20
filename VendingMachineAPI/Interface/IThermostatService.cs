namespace VendingMachineAPI.Interface
{
    public interface IThermostatService
    {
        bool Isworking();

        Task SetIsworkingAsync(bool working);

        public event Func<bool,Task>? StatusChange;
    }
}
