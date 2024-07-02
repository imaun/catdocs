using System.CommandLine;

namespace Spece.Commands;

public static class MainCommand
{
    private static RootCommand _command = new();

    private static Option<string> _pathOption =
        new Option<string>(name: "path", description: "The path to OpenAPI spec file.");

    public static async Task InvokeAsync(string[] args)
    {
        _command.AddOption(_pathOption);
        
        _command.SetHandler((path) =>
        {
            Console.WriteLine($"Hello {path}");
        }, _pathOption);

        await _command.InvokeAsync(args);
    }
}