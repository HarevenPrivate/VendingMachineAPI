namespace VendingMachineAPI.Interface
{
    public interface IThermostatService
    {
        bool Isworking();

        Task SetIsworking(bool working);

        public event Action<bool>? StatusChange;
    }
}
