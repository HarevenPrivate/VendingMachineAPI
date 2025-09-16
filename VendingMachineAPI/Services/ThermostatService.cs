using VendingMachineAPI.Interface;
using VendingMachineAPI.Models;
namespace VendingMachineAPI.Services
{
    
    public class ThermostatService(IPanelService panelService) : IThermostatService
    {
        private readonly SemaphoreSlim _lock = new(1,1);
        bool _isWorking = true;
        
        public event Func<bool,Task>? StatusChange;

        public bool Isworking()
        {
            lock (_lock)
            {
                return _isWorking;
            }
        }

        public async Task SetIsworking(bool working)
        {
            await _lock.WaitAsync();
            try
            {
                if (_isWorking != working)
                {
                    _isWorking = working;
                    if(StatusChange != null) 
                        await StatusChange(working);
                }


                if (_isWorking)
                {
                    await panelService.NotifyPanelAsync("System ready please select a product ", new VendingState() { ThermostatWorking = true });
                }
                else
                {
                    await panelService.NotifyPanelAsync("System out of order ", new VendingState() { ThermostatWorking = false });
                }
            }
            finally
            {
                _lock.Release();
            }
        }

    }
}
