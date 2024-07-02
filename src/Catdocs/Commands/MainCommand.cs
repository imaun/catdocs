using System.CommandLine;

namespace Catdocs.Commands;

public static class MainCommand
{
    private static RootCommand _command = new();

    private static Option<string> _pathOption = new(
        name: "path", 
        description: "The path to OpenAPI spec file.");

    private static Argument<string> _openApiVersion = new(
        "--version",
        getDefaultValue: () => "3.0",
        description: "OpenAPI Spec version (2.0 or 3.0)");

    private static Argument<string> _openApiFormat = new(
        "--format",
        getDefaultValue: () => "yaml",
        description: "OpenAPI Spec format (yaml or json)");

    public static async Task InvokeAsync(string[] args)
    {
        _command.AddOption(_pathOption);
        
        _command.SetHandler((path, version, format) =>
        {
            Console.WriteLine($"Hello {path}");
        }, _pathOption, _openApiVersion, _openApiFormat);

        await _command.InvokeAsync(args);
    }
}