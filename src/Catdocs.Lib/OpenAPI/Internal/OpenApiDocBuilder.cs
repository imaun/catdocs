using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocBuilder
{
    private readonly OpenApiDocument _document;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private string _inputDir;

    public OpenApiDocBuilder(string inputDir, OpenApiDocument document, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(inputDir);
        ArgumentNullException.ThrowIfNull(document);

        _inputDir = inputDir;
        _version = version;
        _format = format;
    }



    internal Dictionary<string, T> ResolveReferences<T>(string elementType) where T: IOpenApiReferenceable
    {
        var result = new Dictionary<string, T>();
        var file_ext = _format.GetFormatFileExtension();
        var element_dir = typeof(T).GetOpenApiElementDirectoryName();

        var files = Directory.GetFiles(_inputDir, $"*.{file_ext}");
        if (!files.Any())
        {
            return result;
        }

        foreach (var f in files)
        {
            var reader = new OpenApiStreamReader();
            using var file_stream = new FileStream(f, FileMode.Open, FileAccess.Read);
            var componentPart = reader.Read(file_stream, out var diagnostics);
            if (diagnostics is not null)
            {
                SpecLogger.LogError(diagnostics.GetErrorLogForElementType(elementType, f));
            }
        }

        return result;
    }
}