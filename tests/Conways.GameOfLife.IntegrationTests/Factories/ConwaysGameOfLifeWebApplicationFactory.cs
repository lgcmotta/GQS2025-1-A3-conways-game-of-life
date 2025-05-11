[assembly: AssemblyFixture(typeof(ConwaysGameOfLifeWebApplicationFactory))]

namespace Conways.GameOfLife.IntegrationTests.Factories;

public class ConwaysGameOfLifeWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("mirror.gcr.io/postgres")
        .WithPortBinding(5432, true)
        .WithDatabase("ConwaysGameOfLife")
        .Build();

    private readonly Dictionary<string, string?> _envs = new()
    {
        ["SERVICE_NAME"] = "conways-game-of-life-api",
        ["SERVICE_NAMESPACE"] = "conways-game-of-life",
        ["SERVICE_VERSION"] = "1.0.0",
        ["AUTOGENERATE_SERVICE_INSTANCE_ID"] = "true",
        ["EXPORTER_ENDPOINT"] = "http://localhost:5431/ingest/otlp/v1/logs",
        ["SEQ_ENDPOINT"] = "http://localhost:4317",
        ["SEQ_API_KEY"] = Guid.NewGuid().ToString()
    };

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<BoardDbContext>();

        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

#pragma warning disable CA1816
    public new async ValueTask DisposeAsync() => await base.DisposeAsync();
#pragma warning restore CA1816

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        foreach (var (key, value) in _envs)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        builder.ConfigureServices(services =>
        {
            RemoveDbContextOptions<BoardDbContext>(services);

            RemoveDbContextOptions<BoardDbContextReadOnly>(services);

            var connectionString = _container.GetConnectionString();

            services.AddDbContext<BoardDbContext>((provider, optionsBuilder) =>
            {
                optionsBuilder.UseNpgsql(connectionString, pgsql =>
                {
                    pgsql.EnableRetryOnFailure(3);
                });

                var interceptors = InterceptorsAssemblyScanner.Scan(provider, typeof(BoardDbContext).Assembly);

                optionsBuilder.AddInterceptors(interceptors);
            });

            services.AddDbContext<BoardDbContextReadOnly>(optionsBuilder =>
            {
                optionsBuilder.UseNpgsql(connectionString, pgsql =>
                {
                    pgsql.EnableRetryOnFailure(3);
                });
            });
        });
    }

    private static void RemoveDbContextOptions<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var serviceDescriptor = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(DbContextOptions<TDbContext>)
        );

        if (serviceDescriptor is not null)
        {
            services.Remove(serviceDescriptor);
        }
    }
}