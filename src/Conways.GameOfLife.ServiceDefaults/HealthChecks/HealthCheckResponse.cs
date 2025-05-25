namespace Microsoft.Extensions.Hosting.HealthChecks;

public sealed record HealthCheckResponse
{
    public required string Status { get; init; }

    public required TimeSpan TotalDuration { get; init; }

    public Dictionary<string, HealthCheckEntry>? Entries { get; init; }
}