using Microsoft.AspNetCore.SignalR;
using VendingMachineAPI.Hubs;
using VendingMachineAPI.Interface;

namespace VendingMachineAPI.Services
{
    public class MoneyDeviceService: IMoneyDeviceService
    {
        private decimal _amount = 0;
        private readonly SemaphoreSlim _lock = new(1,1);
        private readonly SemaphoreSlim _lockWorking = new(1, 1);

        public event Func<decimal,Task>? BalanceChanged;
        private readonly IHubContext<MoneyHub> _moneyHub;
        private readonly IThermostatService _thermostatService;

        public MoneyDeviceService(IHubContext<MoneyHub> moneyHub, IThermostatService thermostatService)
        {
            _moneyHub = moneyHub;
            _thermostatService = thermostatService;
            _thermostatService.StatusChange += ThermostatService_StatusChange;
        }


        private async Task ThermostatService_StatusChange(bool working)
        {
            await _lockWorking.WaitAsync();
            try
            {


                if (!working)
                {
                    decimal refund = _amount;
                    await RefundAsync();
                    await NotifyMoneyAsync($"Refund: {refund} Total balance 0 \n System out of ordr ", refund);
                }
                else
                {
                    await NotifyMoneyAsync($"Please insert Money .... ", _amount);
                }
            }
            finally
            {
                _lockWorking.Release();
            }
        }
        public async Task InsetMoneyAsync(decimal amount)
        {
            await _lockWorking.WaitAsync();
            try
            {
                await _lock.WaitAsync();
                try
                {
                    _amount += amount;
                }
                finally
                {
                    _lock.Release();
                }
               

                await NotifyMoneyAsync($"Total balance: {_amount}", _amount);

                if (!_thermostatService.Isworking())
                {
                    await RefundAsync();
                }
                else
                {
                    // Fire the event when money is inserted
                    if(BalanceChanged != null)
                        await BalanceChanged(_amount);
                }
            }
            finally
            {
                _lockWorking.Release();
            }
        }

        public async Task NotifyMoneyAsync(string message, decimal total)
        {
            await _moneyHub.Clients.All.SendAsync("MoneyUpdate", new
            {
                Timestamp = DateTime.UtcNow,
                Message = message,
                Total = total
            });
        }
        

        public async Task<decimal> GetBalance()
        {
            await _lock.WaitAsync();
            try
            {
                return _amount;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async  Task RefundAsync(decimal price = 0)
        {
            decimal refund;
            await _lock.WaitAsync();
            try
            {
                refund = _amount - price;
                _amount = 0;
                await NotifyMoneyAsync($"Refund: {refund} Total balance 0", refund);
            }
            finally
            {
                _lock.Release();
            }
            
        }
    }
}
