using Conways.GameOfLife.Infrastructure.Extensions;
using Conways.GameOfLife.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Conways.GameOfLife.Infrastructure.Factories;

public class BoardDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BoardDbContext>
{
    public BoardDbContext CreateDbContext(string[] args)
    {
        var userSecretsId = args.GetValueOrDefault<string>("--user-secrets-id");

        var configuration = ConfigurationFactory.CreateConfiguration(userSecretsId);

        var connectionString = configuration.GetConnectionString(BoardDbContext.DatabaseName);

        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure(3))
            .Options;

        return new BoardDbContext(options);
    }
}