using Microsoft.AspNetCore.Mvc;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;

namespace VendingMachineAPI.Controllers;

[ApiController]
[Route("api/thermostat")]
public class ThermostatController(IThermostatService thermostatService) : ControllerBase
{
    [HttpPost("status")]
    public  async Task<IActionResult> Status([FromBody] ThermostatDto dto)
    {
        await thermostatService.SetIsworkingAsync( dto.Working);
        return Accepted();
    }

    public record ThermostatDto(bool Working);
}
