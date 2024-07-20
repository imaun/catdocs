using System.CommandLine;

namespace Catdocs.Commands;

public static class ConvertCommand
{
    private static Command _command =
        new("convert", "Converts an OpenAPI document from/to json or yaml formats.");

    private static Option<FileInfo> _fileOption = new(
        aliases: ["--file", "--source", "-s", "--spec"],
        description: "The path to OpenAPI spec file."
        );
    
    private static Option<string> _openApiVersionArg = new(
        aliases: ["--spec-version", "--spec-ver", "-v"],
        getDefaultValue: () => "3.0",
        description: "OpenAPI Spec version (2.0 or 3.0)"
        );

    private static Option<string> _openApiFormatArg = new(
        aliases: ["--format", "--f", "-f"],
        getDefaultValue: () => "yaml",
        description: "OpenAPI Spec format (yaml or json)"
        );

    private static Option<string> _outputFileArg = new(
        aliases: ["--output", "-out"],
        description: "The Output file"
        );


    public static Command GetCommand()
    {
        _command.AddOption(_fileOption);
        _command.AddOption(_openApiFormatArg);
        _command.AddOption(_openApiVersionArg);
        _command.AddOption(_outputFileArg);


        return _command;
    }
    
    
}