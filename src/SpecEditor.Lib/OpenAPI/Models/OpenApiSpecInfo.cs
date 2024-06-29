namespace SpecEditor.Lib.OpenAPI;

public record OpenApiSpecInfo(
    string InputFile,
    bool HasErrors,
    string Version,
    string Format,
    bool IsJson,
    bool IsYaml,
    List<string> Errors
    );