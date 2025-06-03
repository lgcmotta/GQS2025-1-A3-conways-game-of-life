using Conways.GameOfLife.API.Behaviors;
using Conways.GameOfLife.Infrastructure;
using Conways.GameOfLife.Infrastructure.Persistence;
using FluentValidation;

namespace Conways.GameOfLife.API.Extensions;

public static class ServiceCollectionExtensions
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
}