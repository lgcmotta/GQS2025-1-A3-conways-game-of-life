namespace Microsoft.Extensions.Hosting.HealthChecks;

internal class ReadinessBackgroundService : BackgroundService
{
    private readonly ReadinessHealthCheck _readinessCheck;

    public ReadinessBackgroundService(ReadinessHealthCheck readinessCheck)
    {
        _readinessCheck = readinessCheck;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _readinessCheck.StartupCompleted = true;
        return Task.CompletedTask;
    }
}