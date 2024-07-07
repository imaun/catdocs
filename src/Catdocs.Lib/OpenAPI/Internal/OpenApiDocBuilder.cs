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


    public OpenApiDocument Build()
    {
        var api_paths = new OpenApiPaths();
        foreach (var path in _document.Paths)
        {
            if (path.Value.Reference is not null)
            {
                var pathRef = path.Value.Reference.ExternalResource;
                var pathDoc = LoadApiPathDocument(Path.Combine(_inputDir, pathRef));
                foreach (var resolvedPath in pathDoc.Paths)
                {
                    api_paths.Add(resolvedPath.Key, resolvedPath.Value);
                }
            }
            else
            {
                api_paths.Add(path.Key, path.Value);
            }
        }

        _document.Paths = api_paths;
        
        var resolvedComponents = new OpenApiComponents();
        resolvedComponents.Schemas = ResolveReferences<OpenApiSchema>();
        resolvedComponents.Callbacks = ResolveReferences<OpenApiCallback>();
        resolvedComponents.Examples = ResolveReferences<OpenApiExample>();
        resolvedComponents.Parameters = ResolveReferences<OpenApiParameter>();
        resolvedComponents.Headers = ResolveReferences<OpenApiHeader>();
        resolvedComponents.Responses = ResolveReferences<OpenApiResponse>();
        resolvedComponents.RequestBodies = ResolveReferences<OpenApiRequestBody>();
        resolvedComponents.Links = ResolveReferences<OpenApiLink>();

        _document.Components = resolvedComponents;

        return _document;
    }


    internal OpenApiDocument LoadApiPathDocument(string filePath)
    {
        var reader = new OpenApiStreamReader();
        using var file_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var doc = reader.Read(file_stream, out var diagnostic);
        if (diagnostic is not null)
        {
            SpecLogger.LogError(diagnostic.GetErrorLogForElementType(OpenApiConstants.Path, filePath));
        }

        return doc;
    }

    internal Dictionary<string, T> ResolveReferences<T>() where T: IOpenApiReferenceable
    {
        var elementType = typeof(T).GetOpenApiElementTypeName();
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

            foreach (var component in componentPart.GetComponentsWithType<T>(elementType))
            {
                result.Add(component.Key, component.Value);
            }
        }

        return result;
    }
}