using Asp.Versioning;
using Conways.GameOfLife.API.Behaviors;
using Conways.GameOfLife.API.Diagnostics;
using Conways.GameOfLife.API.Middlewares;
using Conways.GameOfLife.Infrastructure.Persistence;
using FluentValidation;

namespace Conways.GameOfLife.API.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<Program>();
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.Lifetime = ServiceLifetime.Scoped;
        });

        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    internal static IServiceCollection AddPostgreHealthCheck(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(BoardDbContext.DatabaseName);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddHealthChecks().AddNpgSql(connectionString);

        return services;
    }

    internal static IServiceCollection AddApiVersioning(this IServiceCollection services, ApiVersion version)
    {
        services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = version;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            })
            .EnableApiVersionBinding();

        return services;
    }

    internal static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        return services.AddExceptionHandler<ExceptionHandler>()
            .AddTransient<ExceptionMiddleware>();
    }
}