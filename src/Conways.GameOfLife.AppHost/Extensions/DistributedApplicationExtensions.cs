namespace Conways.GameOfLife.AppHost.Extensions;

internal static class DistributedApplicationExtensions
{
    private const string PowerShellExecutable = "pwsh";
    private const string BashShellExecutable = "bash";
    private const string WorkingDirectory = "../../";
    private static readonly string[] TargetArgs = ["--target", "Apply-Migrations-Bundle"];

    internal static IResourceBuilder<ExecutableResource> AddDotNetCakeEfCoreMigrator(this IDistributedApplicationBuilder builder, string name)
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return builder.AddMigratorScriptForLinuxOrMacOS(name);
        }

        return builder.AddMigratorScriptForWindows(name);
    }

    private static IResourceBuilder<ExecutableResource> AddMigratorScriptForWindows(
        this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddExecutable(name, PowerShellExecutable, WorkingDirectory, ["./build.ps1", ..TargetArgs]);
    }

    private static IResourceBuilder<ExecutableResource> AddMigratorScriptForLinuxOrMacOS(
        this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddExecutable(name, BashShellExecutable, WorkingDirectory, ["./build.sh", ..TargetArgs]);
    }
}