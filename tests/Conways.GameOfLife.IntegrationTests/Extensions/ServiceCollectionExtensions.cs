namespace Conways.GameOfLife.IntegrationTests.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void AddDbContextForTestContainers<TDbContext>(
        this IServiceCollection services, string connectionString, bool useInterceptors = false)
        where TDbContext : DbContext
    {
        services.RemoveDbContextOptions<TDbContext>();

        services.AddDbContext<TDbContext>((provider, optionsBuilder) =>
        {
            optionsBuilder.UseNpgsql(connectionString, pgsql => { pgsql.EnableRetryOnFailure(3); });

            if (!useInterceptors) return;

            var interceptors = InterceptorsAssemblyScanner.Scan(provider, typeof(TDbContext).Assembly);

            optionsBuilder.AddInterceptors(interceptors);
        });
    }

    private static void RemoveDbContextOptions<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(DbContextOptions<TDbContext>));

        if (serviceDescriptor is not null)
        {
            services.Remove(serviceDescriptor);
        }
    }
}