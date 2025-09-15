using Microsoft.AspNetCore.Mvc;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;

namespace VendingMachineAPI.Controllers;

[ApiController]
[Route("api/thermostat")]
public class ThermostatController(IThermostatService thermostatService) : ControllerBase
{
    [HttpPost("status")]
    public IActionResult Status([FromBody] ThermostatDto dto)
    {
        thermostatService.SetIsworking( dto.Working);
        return Accepted();
    }

    public record ThermostatDto(bool Working);
}
