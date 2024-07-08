using System.CommandLine;

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
        aliases: ["--outputDir", "-outDir"],
        description: "The Output directory");
    
    
}