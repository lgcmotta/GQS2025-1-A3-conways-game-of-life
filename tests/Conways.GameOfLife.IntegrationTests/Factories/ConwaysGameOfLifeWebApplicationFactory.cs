[assembly: AssemblyFixture(typeof(ConwaysGameOfLifeWebApplicationFactory))]

namespace Conways.GameOfLife.IntegrationTests.Factories;

public class ConwaysGameOfLifeWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("mirror.gcr.io/postgres")
        .WithPortBinding(5432, true)
        .WithDatabase("ConwaysGameOfLife")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<BoardDbContext>();

        await context.Database.MigrateAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    public new async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();

        await base.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    public HttpClient CreateHttpClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = Server.BaseAddress
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var connectionString = _container.GetConnectionString();

            services.AddDbContextForTestContainers<BoardDbContext>(connectionString, useInterceptors: true);
            services.AddDbContextForTestContainers<BoardDbContextReadOnly>(connectionString);
        });
    }
}