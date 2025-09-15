using VendingMachineAPI.Models;

namespace VendingMachineAPI.Interface;

public interface IPanelService
{
    Task NotifyPanelAsync(string message, VendingState state);
}

