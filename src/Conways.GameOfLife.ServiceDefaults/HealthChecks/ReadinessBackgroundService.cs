using Microsoft.Extensions.Diagnostics.HealthChecks;

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

internal class ReadinessHealthCheck : IHealthCheck
{
    private volatile bool _isReady;

    public bool StartupCompleted
    {
        get => _isReady;
        set => _isReady = value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(StartupCompleted
            ? HealthCheckResult.Healthy("Startup task completed")
            : HealthCheckResult.Unhealthy("Startup task is still running"));
    }
}