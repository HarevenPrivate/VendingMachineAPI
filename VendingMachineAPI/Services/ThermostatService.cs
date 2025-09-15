using VendingMachineAPI.Interface;
using VendingMachineAPI.Models;
namespace VendingMachineAPI.Services
{
    
    public class ThermostatService(IPanelService panelService) : IThermostatService
    {
        private readonly Lock _lock = new();
        bool _isWorking = true;

        public event Action<bool>? StatusChange;

        public bool Isworking()
        {
            lock (_lock)
            {
                return _isWorking;
            }
        }

        public async Task SetIsworking(bool working)
        {
            lock (_lock)
            {
                if( _isWorking != working)
                {
                    _isWorking = working;
                    StatusChange?.Invoke(working);
                }
                
            }

            if (_isWorking) {
                await panelService.NotifyPanelAsync("System ready please select a product ", new VendingState() { ThermostatWorking=true });
            }
            else
            {
                await panelService.NotifyPanelAsync("System out of order ", new VendingState() { ThermostatWorking = false });
            }
        }

    }
}
