using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Models;
using VendingMachineAPI.Services;

namespace VendingMachineAPI.Controllers;

[ApiController]
[Route("api/keyboard")]
public class KeyboardController(Channel<VendingEvent> channel) : ControllerBase
{
    //private readonly IKeyboardService _keyboardService = keyboardService;
    //private readonly Channel<VendingEvent> _channel;

    [HttpPost("press")]
    public async Task<IActionResult> Press([FromBody] KeyDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Key))
            return BadRequest("Key required");

        var key = dto.Key.Trim().ToUpperInvariant();

        if (key == "OK")
            await channel.Writer.WriteAsync(new OkEvent());
        else if (key == "CANCEL")
            await channel.Writer.WriteAsync(new CancelEvent());
        else
            await channel.Writer.WriteAsync(new KeyPressEvent(dto.Key));

        return Accepted();
    }

    public record KeyDto(string Key);
}
