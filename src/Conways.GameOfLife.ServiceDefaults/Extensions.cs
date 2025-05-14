using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
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
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
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
            .WithTags("Health Checks");

        app.MapHealthCheck(LivenessEndpointPath, check => !check.Tags.Contains("ready"))
            .WithName("liveness-health-check")
            .WithDisplayName("Liveness Health Check")
            .WithTags("Health Checks");

        return app;
    }

    private static RouteHandlerBuilder MapHealthCheck(this WebApplication app, [StringSyntax("uri")] string pattern, Func<HealthCheckRegistration, bool> predicate)
    {
        return app.MapGet(pattern, async (HealthCheckService service, CancellationToken cancellationToken = default) =>
        {
            var report = await service.CheckHealthAsync(predicate, cancellationToken);

            var response = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.ToString("c"),
                Entries = report.Entries.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        Status = kvp.Value.Status.ToString(),
                        kvp.Value.Description,
                        Duration = kvp.Value.Duration.ToString("c"),
                        Exception = kvp.Value.Exception?.Message,
                        kvp.Value.Data
                    })
            };

            return report.Status switch
            {
                HealthStatus.Healthy => Results.Ok(response),
                HealthStatus.Degraded => Results.Ok(response),
                HealthStatus.Unhealthy => Results.Ok(response),
                _ => Results.Ok(response)
            };
        });
    }
}