using Microsoft.OpenApi.Models;

namespace Catdocs.Lib.OpenAPI;

public record OpenApiSpecInfo(
    string InputFilePath,
    OpenApiDocument Document,
    bool HasErrors,
    string Version,
    string Format,
    bool IsJson,
    bool IsYaml,
    List<string> Errors,
    long ParseTime
)
{
    public bool Success => !HasErrors;
}