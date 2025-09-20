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

        public async Task SetIsworkingAsync(bool working)
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
                    await panelService.NotifyPanelAsync("System ready please select a product ... ");
                }
                else
                {
                    await panelService.NotifyPanelAsync("System out of order");
                }
            }
            finally
            {
                _lock.Release();
            }
        }

    }
}
