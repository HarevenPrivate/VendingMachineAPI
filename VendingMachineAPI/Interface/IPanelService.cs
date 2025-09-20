namespace VendingMachineAPI.Interface;

public interface IPanelService
{
    Task NotifyPanelAsync(string message);
}

