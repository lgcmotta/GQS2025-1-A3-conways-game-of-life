namespace Microsoft.Extensions.Hosting.HealthChecks;

public sealed record HealthCheckEntry
{
    public required string Status { get; init; }

    public string? Description { get; init; }

    public required TimeSpan Duration { get; init; }

    public string? Exception { get; init; }

    public IReadOnlyDictionary<string, object>? Data { get; init; }
}