using System.CommandLine;
using Catdocs.Lib.OpenAPI;
using Microsoft.OpenApi;

namespace Catdocs.Commands;

public static class BuildCommand
{
    private static Command _command =
        new("build", description: "Build an OpenAPI document from multiple referenced files.");

    private static Option<FileInfo> _fileOption = new(
        aliases: ["--file", "--source", "-s", "--spec"],
        description: "The path to OpenAPI spec file.");
    
    private static Option<string> _openApiVersionArg = new(
        aliases: ["--spec-version", "--spec-ver", "-v"],
        getDefaultValue: () => "3.0",
        description: "OpenAPI Spec version (2.0 or 3.0)");

    private static Option<string> _openApiFormatArg = new(
        aliases: ["--format", "--f", "-f"],
        getDefaultValue: () => "yaml",
        description: "OpenAPI Spec format (yaml or json)");

    private static Option<string> _outputDirArg = new(
        aliases: ["--output", "-out"],
        description: "The Output file");


    public static Command GetCommand()
    {
        _command.AddOption(_fileOption);
        _command.AddOption(_openApiVersionArg);
        _command.AddOption(_openApiFormatArg);
        _command.AddOption(_outputDirArg);
        
        _command.SetHandler(Run, _fileOption, _openApiVersionArg, _openApiFormatArg, _outputDirArg);

        return _command;
    }


    public static void Run(FileInfo file, string version, string format, string outputFile)
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
        
        var spec_version = OpenApiSpecVersion.OpenApi3_0;
        var spec_format = OpenApiFormat.Yaml;
        
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
        
        parser.Build(outputFile);
        Console.WriteLine($"Build took: {parser.BuildTime} ms");
    }
}