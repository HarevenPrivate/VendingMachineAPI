using Microsoft.AspNetCore.Mvc;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;

namespace VendingMachineAPI.Controllers;

[ApiController]
[Route("api/keyboard")]
public class KeyboardController(IKeyboardService keyboardService) : ControllerBase
{
    private readonly IKeyboardService _keyboardService = keyboardService;

    [HttpPost("press")]
    public async Task<IActionResult> Press([FromBody] KeyDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Key))
            return BadRequest("Key required");

        var key = dto.Key.Trim().ToUpperInvariant();

        if (key == "OK")
            await _keyboardService.EnqueueOkAsync();
        else if (key == "CANCEL")
            await _keyboardService.EnqueueCancelAsync();
        else
            await _keyboardService.EnqueueKeyPressAsync(key);

        return Accepted();
    }

    public record KeyDto(string Key);
}
