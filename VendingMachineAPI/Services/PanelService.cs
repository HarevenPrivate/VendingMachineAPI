using Microsoft.AspNetCore.SignalR;
using VendingMachineAPI.Hubs;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Models;


namespace VendingMachineAPI.Services;

public record PanelUpdate(DateTime Timestamp, string Message);



public class PanelService(IHubContext<PanelHub> hubContext) : IPanelService
{
    public async Task NotifyPanelAsync(string message)
    {
        await hubContext.Clients.All.SendAsync("PanelUpdate", new
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
        });
    }


}

