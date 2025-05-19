using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string LivenessEndpointPath = "/healthz/live";
    private const string ReadinessEndpointPath = "/healthz/ready";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddEventCountersInstrumentation(options => options
                        .AddEventSources(
                             "Microsoft.AspNetCore.Hosting",
                            "Microsoft.AspNetCore.Http.Connections",
                            "Microsoft-AspNetCore-Server-Kestrel",
                            "System.Net.Http",
                            "System.Net.NameResolution",
                            "System.Net.Security"
                        )
                    );
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(ReadinessEndpointPath) &&
                            !context.Request.Path.StartsWithSegments(LivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    private static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSingleton<ReadinessHealthCheck>();

        builder.Services.AddHealthChecks().AddCheck<ReadinessHealthCheck>("readiness", tags: ["ready"]);

        builder.Services.AddHostedService<ReadinessBackgroundService>();

        return builder;
    }

    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {
        app.MapHealthCheck(ReadinessEndpointPath, check => !check.Tags.Contains("ready"))
            .WithName("readiness-health-check")
            .WithDisplayName("Readiness Health Check")
            .WithTags("Health Checks")
            .Produces<HealthCheckResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<HealthCheckResponse>(StatusCodes.Status503ServiceUnavailable, contentType: MediaTypeNames.Application.ProblemJson);

        app.MapHealthCheck(LivenessEndpointPath, check => !check.Tags.Contains("ready"))
            .WithName("liveness-health-check")
            .WithDisplayName("Liveness Health Check")
            .WithTags("Health Checks")
            .Produces<HealthCheckResponse>(contentType: MediaTypeNames.Application.Json)
            .Produces<HealthCheckResponse>(StatusCodes.Status503ServiceUnavailable, contentType: MediaTypeNames.Application.ProblemJson);

        return app;
    }

    private static RouteHandlerBuilder MapHealthCheck(this WebApplication app, [StringSyntax("uri")] string pattern, Func<HealthCheckRegistration, bool> predicate)
    {
        return app.MapGet(pattern, async Task (HttpContext context, HealthCheckService service, CancellationToken cancellationToken = default) =>
        {
            var report = await service.CheckHealthAsync(predicate, cancellationToken);

            var response = new HealthCheckResponse
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.ToDictionary(
                    pair => pair.Key,
                    pair => new HealthCheckEntry
                    {
                        Status = pair.Value.Status.ToString(),
                        Description = pair.Value.Description,
                        Duration = pair.Value.Duration,
                        Exception = pair.Value.Exception?.Message,
                        Data = pair.Value.Data
                    })
            };

            var (statusCode, contentType) = report.Status switch
            {
                HealthStatus.Healthy => (StatusCodes.Status200OK, MediaTypeNames.Application.Json),
                HealthStatus.Degraded => (StatusCodes.Status200OK, MediaTypeNames.Application.ProblemJson),
                HealthStatus.Unhealthy => (StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson),
                _ => (StatusCodes.Status200OK, MediaTypeNames.Application.Json),
            };

            context.Response.ContentType = contentType;
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response, JsonSerializerOptions.Web, cancellationToken);
        });
    }
}