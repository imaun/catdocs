using System.CommandLine;
using Catdocs.Lib.OpenAPI;
using Microsoft.OpenApi;

namespace Catdocs.Commands;

public static class MainCommand
{
    private static RootCommand _command = new("stats");

    private static Option<FileInfo> _fileOption = new(
        aliases: ["--file", "--source", "--spec", "-s"], 
        description: "The path to OpenAPI spec file.");

    private static Option<string> _openApiVersionArg = new(
        aliases: ["--spec-version", "--spec-ver", "-v"],
        getDefaultValue: () => "3.0",
        description: "OpenAPI Spec version (2.0 or 3.0)");

    private static Option<string> _openApiFormatArg = new(
        aliases: ["--format", "--f", "-f"],
        getDefaultValue: () => "yaml",
        description: "OpenAPI Spec format (yaml or json)");

    public static async Task InvokeAsync(string[] args)
    {
        _command.AddOption(_fileOption);
        _command.AddOption(_openApiVersionArg);
        _command.AddOption(_openApiFormatArg);
        
        _command.SetHandler(Run, _fileOption, _openApiVersionArg, _openApiFormatArg);
        
        _command.AddCommand(SplitCommand.GetCommand());
        _command.AddCommand(BuildCommand.GetCommand());
        _command.AddCommand(ConvertCommand.GetCommand());

        await _command.InvokeAsync(args);
    }

    private static void Run(FileInfo file, string version, string format)
    {
        if (file is null)
        {
            Console.WriteLine("Error: Filename is not valid!");
            return;
        }

        if (!file.Exists)
        {
            Console.WriteLine($"Error: File '{file.Name}' not found!");
            return;
        }
        
        OpenApiSpecVersion spec_version;
        OpenApiFormat spec_format;

        if (version == "2.0" || version == "2")
        {
            spec_version = OpenApiSpecVersion.OpenApi2_0;
        }
        else if (version == "3.0" || version == "3")
        {
            spec_version = OpenApiSpecVersion.OpenApi3_0;
        }
        else
        {
            Console.WriteLine("Error: OpenApiSpec Version is not valid!");
            return;
        }

        if (format.ToLower() == "json")
        {
            spec_format = OpenApiFormat.Json;
        }
        else if (format.ToLower() == "yaml")
        {
            spec_format = OpenApiFormat.Yaml;
        }
        else
        {
            Console.WriteLine("Error: OpenApiSpec format is not valid!");
            return;
        }

        var parser = new OpenAPISpecParser(
            file.FullName, spec_version, spec_format, true, true);

        var parse_result = parser.Load();
        if (parse_result.HasErrors)
        {
            ConsoleExtensions.WriteErrorLine("🩻 Found some errors: ");
            parse_result.Errors.WriteListToConsole(useLineNo: true, useTab: true, color: ConsoleColor.Red);
            
            return;
        }

        var stats = parser.GetStats();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"File: {file.FullName}");
        Console.WriteLine("Status: ✅ OK");
        Console.ResetColor();
        Console.WriteLine();
        
        stats.WriteToConsole();
    }
}