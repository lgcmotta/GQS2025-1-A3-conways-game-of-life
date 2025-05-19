using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.Hosting.HealthChecks;

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