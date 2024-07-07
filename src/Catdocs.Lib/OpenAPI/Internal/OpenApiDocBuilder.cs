using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

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
            
        }
    }
}