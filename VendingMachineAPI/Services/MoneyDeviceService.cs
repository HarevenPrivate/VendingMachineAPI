using Microsoft.AspNetCore.SignalR;
using VendingMachineAPI.Hubs;
using VendingMachineAPI.Interface;

namespace VendingMachineAPI.Services
{
    public class MoneyDeviceService: IMoneyDeviceService
    {
        private decimal _amount = 0;
        private readonly Lock _lock = new();

        public event Action<decimal>? BalanceChanged;
        private IHubContext<MoneyHub> _moneyHub;
        private IThermostatService _thermostatService;

        public MoneyDeviceService(IHubContext<MoneyHub> moneyHub, IThermostatService thermostatService)
        {
            _moneyHub = moneyHub;
            _thermostatService = thermostatService;
            _thermostatService.StatusChange += _thermostatService_StatusChange;
        }

        private void _thermostatService_StatusChange(bool working)
        {
            if (!working)
            {
                decimal refund = _amount;
                RefundAsync();
                NotifyMoneyAsync($"Refund: {refund} Total balance 0 \n System out of ordr ", refund);
            }
            else
            {
                NotifyMoneyAsync($"Please insert Money .... ",_amount);
            }
            
        }

        public async Task InsetMoneyAsync(decimal amount)
        {
            
            lock (_lock)
            {
                _amount += amount;
            }
            await NotifyMoneyAsync($"Total balance: {_amount}", _amount);

            if (!_thermostatService.Isworking())
            {
                await RefundAsync();
            }
            else
            {
                // Fire the event when money is inserted
                BalanceChanged?.Invoke(_amount);
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
        

        public decimal GetBalance()
        {
            lock (_lock)
            {
                return _amount;
            }
        }

        public async  Task RefundAsync(decimal price = 0)
        {
            decimal refund;
            lock (_lock)
            {
                refund = _amount - price;
                _amount = 0;
            }
            await NotifyMoneyAsync($"Refund: {refund} Total balance 0", refund);
        }
    }
}
