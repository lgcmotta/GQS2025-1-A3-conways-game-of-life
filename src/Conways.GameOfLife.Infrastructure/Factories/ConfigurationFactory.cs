using Microsoft.Extensions.Configuration;

namespace Conways.GameOfLife.Infrastructure.Factories;

internal static class ConfigurationFactory
{
    internal static IConfiguration CreateConfiguration(string? userSecretsId = null)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder();

        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false);
        }

        if (!string.IsNullOrWhiteSpace(userSecretsId))
        {
            builder.AddUserSecrets(userSecretsId);
        }

        builder.AddEnvironmentVariables();

        return builder.Build();
    }
}