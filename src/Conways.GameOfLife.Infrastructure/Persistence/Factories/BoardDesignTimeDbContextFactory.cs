using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.CommandLine;

namespace Conways.GameOfLife.Infrastructure.Persistence.Factories;

public class BoardDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BoardDbContext>
{
    public BoardDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.GetValueOrDefault<string>("--pg-connection-string");

        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure(3))
            .Options;

        return new BoardDbContext(options);
    }
}

static file class CommandLineArgumentsExtensions
{
    internal static T? GetValueOrDefault<T>(this string[] args, string key)
    {
        var value = default(T);
        try
        {
            var option = new Option<T>(key);
            var command = new RootCommand();
            command.AddOption(option);
            command.SetHandler(handle: v => value = v, option);
            command.Invoke(args);

            return value;
        }
        catch
        {
            return value;
        }
    }
}