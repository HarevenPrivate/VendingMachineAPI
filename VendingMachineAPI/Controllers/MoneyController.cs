using Microsoft.AspNetCore.Mvc;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;

namespace VendingMachineAPI.Controllers;

[ApiController]
[Route("api/money")]
public class MoneyController(IMoneyDeviceService moneyService) : ControllerBase
{
    [HttpPost("insert")]
    public async Task<IActionResult> Insert([FromBody] MoneyDto dto)
    {
        if (dto == null || dto.Amount <= 0) return BadRequest("Invalid amount");
        await moneyService.InsetMoneyAsync(dto.Amount);
        return Accepted();
    }

    public record MoneyDto(decimal Amount);
}
