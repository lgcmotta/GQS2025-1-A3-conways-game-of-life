using Conways.GameOfLife.Infrastructure.Persistence;
using Conways.GameOfLife.Infrastructure.Persistence.Interceptors;
using System.Diagnostics.CodeAnalysis;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conways.GameOfLife.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHashIds(this IServiceCollection services, IConfiguration configuration)
    {
        var salt = configuration.GetValue<string>("HashIds:Salt");

        var minHashLength = configuration.GetValue<int>("HashIds:MinHashLength");

        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        services.AddSingleton<IHashids>(new Hashids(salt, minHashLength));

        return services;
    }

    public static IServiceCollection AddBoardDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(BoardDbContext.DatabaseName);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var retries = configuration.GetValue<int>("PostgresSettings:RetryCount");

        services.AddBoardDbContext<BoardDbContext>(connectionString, retries);

        services.AddBoardDbContext<BoardDbContextReadOnly>(connectionString, retries, useInterceptors: false);

        return services;
    }

    private static IServiceCollection AddBoardDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        int retries,
        bool useInterceptors = true)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((provider, builder) =>
        {
            builder.UseNpgsql(connectionString, pgsql => { pgsql.EnableRetryOnFailure(retries); });

            if (!useInterceptors) return;

            var interceptors = InterceptorsAssemblyScanner.Scan(provider, typeof(BoardDbContext).Assembly);

            builder.AddInterceptors(interceptors);
        });

        return services;
    }
}