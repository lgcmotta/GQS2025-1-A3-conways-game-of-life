using System.Diagnostics.CodeAnalysis;
using Conways.GameOfLife.API.Behaviors;
using Conways.GameOfLife.Infrastructure.PostgreSQL;
using FluentValidation;

namespace Conways.GameOfLife.API.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        return services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<Program>();
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.Lifetime = ServiceLifetime.Scoped;
        });
    }

    public static IServiceCollection AddFluentValidators(this IServiceCollection services)
    {
        return services.AddValidatorsFromAssemblyContaining<Program>();
    }

    internal static IServiceCollection AddPostgreHealthCheck(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(BoardDbContext.DatabaseName);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddHealthChecks().AddNpgSql(connectionString);

        return services;
    }
}