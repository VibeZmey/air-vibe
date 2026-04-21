namespace Flights.Infrastructure.Workers;

public class UpdateFlightStatusWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}