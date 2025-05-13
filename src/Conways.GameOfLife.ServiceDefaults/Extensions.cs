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

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
    };

    private const string LivenessEndpointPath = "/healthz/live";
    private const string ReadinessEndpointPath = "/healthz/ready";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

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
                        // Exclude health check requests from tracing
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(ReadinessEndpointPath)
                            && !context.Request.Path.StartsWithSegments(LivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
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

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthCheck(LivenessEndpointPath, check => !check.Tags.Contains("ready"));
        app.MapHealthCheck(ReadinessEndpointPath, check => check.Tags.Contains("ready"));

        return app;
    }


    private static WebApplication MapHealthCheck(
        this WebApplication app,
        [StringSyntax("uri")] string pattern,
        Func<HealthCheckRegistration, bool> predicate)
    {
        app.MapGet(pattern, async Task (HttpContext context) =>
        {
            var service = context.RequestServices.GetRequiredService<HealthCheckService>();

            var report = await service.CheckHealthAsync(predicate, context.RequestAborted);

            var (statusCode, contentType) = report.Status switch
            {
                HealthStatus.Healthy => (StatusCodes.Status200OK, MediaTypeNames.Application.Json),
                HealthStatus.Degraded => (StatusCodes.Status200OK, MediaTypeNames.Application.ProblemJson),
                HealthStatus.Unhealthy => (StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson),
                _ => (StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;

            await context.Response.WriteAsJsonAsync(report, SerializerOptions, context.RequestAborted);
        });

        return app;
    }

}