namespace VendingMachineAPI.Services;

public class EventProcessorHostedService(EventProcessor processor) : BackgroundService
{
    private readonly EventProcessor _processor = processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.RunAsync(stoppingToken);
    }
}

