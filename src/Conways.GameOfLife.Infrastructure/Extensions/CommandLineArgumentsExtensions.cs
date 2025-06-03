using System.CommandLine;

namespace Conways.GameOfLife.Infrastructure.Extensions;

internal static class CommandLineArgumentsExtensions
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