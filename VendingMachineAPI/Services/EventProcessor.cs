using System.Threading.Channels;
using VendingMachineAPI.Models;
using VendingMachineAPI.StateMachine;
using VendingMachineAPI.StateMachine.Interface;

namespace VendingMachineAPI.Services;

public class EventProcessor(Channel<VendingEvent> channel, IVendingContext context)
{
    private readonly Channel<VendingEvent> _channel = channel;
    private readonly IVendingContext _context = context;

    public async Task RunAsync(CancellationToken token)
    {
        await foreach (var ev in _channel.Reader.ReadAllAsync(token))
        {
            switch (ev)
            {
                case KeyPressEvent kp:
                    await _context.OnKeyPressAsync(kp.Key);
                    break;
                case OkEvent:
                    await _context.OnOkAsync();
                    break;
                case CancelEvent:
                    await _context.OnCancelAsync();
                    break;
            }
        }
    }
}

