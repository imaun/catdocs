using Catdocs.Lib.OpenAPI.Extensions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocBuilder
{
    private readonly OpenApiDocument _document;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private string _inputDir;

    public OpenApiDocBuilder(
        string inputDir, 
        OpenApiDocument document, 
        OpenApiSpecVersion version, 
        OpenApiFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDir);
        ArgumentNullException.ThrowIfNull(document);

        _document = document;
        _inputDir = inputDir;
        _version = version;
        _format = format;
    }


    public OpenApiDocument Bundle()
    {
        var api_paths = new OpenApiPaths();
        foreach (var path in _document.Paths)
        {
            if (path.Value is OpenApiPathItemReference reference)
            {
                var pathRef = reference.Reference.ReferenceV3;
                ArgumentException.ThrowIfNullOrWhiteSpace(pathRef);
                var filePath = Path.Combine(_inputDir, pathRef.Split('#', 2)[0]);
                var pathDoc = LoadApiPathDocument(Path.GetFullPath(filePath));
                var resolvedPath = pathDoc.Paths?.Values.SingleOrDefault()
                    ?? throw new InvalidDataException(
                        $"No path item found in referenced file '{filePath}'.");
                api_paths.Add(path.Key, resolvedPath);
            }
            else
            {
                api_paths.Add(path.Key, path.Value);
            }
        }

        _document.Paths = api_paths;

        _document.Components ??= new OpenApiComponents();

        _document.Components.Schemas = ResolveReferences<IOpenApiSchema>();
        _document.Components.Callbacks = ResolveReferences<IOpenApiCallback>();
        _document.Components.Examples = ResolveReferences<IOpenApiExample>();
        _document.Components.Parameters = ResolveReferences<IOpenApiParameter>();
        _document.Components.Headers = ResolveReferences<IOpenApiHeader>();
        _document.Components.Responses = ResolveReferences<IOpenApiResponse>();
        _document.Components.RequestBodies = ResolveReferences<IOpenApiRequestBody>();
        _document.Components.Links = ResolveReferences<IOpenApiLink>();

        return _document;
    }


    internal OpenApiDocument LoadApiPathDocument(string filePath)
    {
        using var file_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var result = OpenApiDocument.LoadAsync(
                file_stream,
                _format.ToStr(),
                OpenApiExtensions.CreateReaderSettings(filePath),
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return result.Document
            ?? throw new InvalidDataException($"Unable to parse referenced document '{filePath}'.");
    }

    internal async Task<OpenApiDocument> LoadApiPathDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var file_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var result = await OpenApiDocument.LoadAsync(
            file_stream,
            _format.ToStr(),
            OpenApiExtensions.CreateReaderSettings(filePath),
            cancellationToken).ConfigureAwait(false);
        return result.Document
            ?? throw new InvalidDataException($"Unable to parse referenced document '{filePath}'.");
    }

    internal Dictionary<string, T> ResolveReferences<T>() where T: IOpenApiReferenceable
    {
        var elementType = typeof(T).GetOpenApiElementTypeName();
        var result = new Dictionary<string, T>();
        var file_ext = _format.GetFormatFileExtension();
        var element_dir = Path.Combine(_inputDir, typeof(T).GetOpenApiElementDirectoryName());

        if (!Directory.Exists(element_dir))
        {
            return result;
        }
        
        var files = Directory.GetFiles(element_dir, $"*.{file_ext}");
        if (!files.Any())
        {
            return result;
        }

        foreach (var f in files)
        {
            var componentPart = LoadApiPathDocument(f);
            // if (diagnostics is not null)
            // {
            //     SpecLogger.LogError(diagnostics.GetErrorLogForElementType(elementType, f));
            // }
            
            foreach (var component in componentPart.GetComponentsWithType<T>(elementType))
            {
                result.Add(component.Key, component.Value);
            }
        }

        return result;
    }

    internal async Task<Dictionary<string, T>> ResolveReferenceAsync<T>(CancellationToken cancellationToken = default) 
        where T : IOpenApiReferenceable
    {
        var elementType = typeof(T).GetOpenApiElementTypeName();
        var result = new Dictionary<string, T>();
        var file_ext = _format.GetFormatFileExtension();
        var element_dir = Path.Combine(_inputDir, typeof(T).GetOpenApiElementDirectoryName());

        if (!Directory.Exists(element_dir))
        {
            return result;
        }
        
        var files = Directory.GetFiles(element_dir, $"*.{file_ext}");
        if (!files.Any())
        {
            return result;
        }

        foreach (var f in files)
        {
            var componentPart = await LoadApiPathDocumentAsync(f, cancellationToken);
            // if (diagnostics is not null)
            // {
            //     SpecLogger.LogError(diagnostics.GetErrorLogForElementType(elementType, f));
            // }
            
            foreach (var component in componentPart.GetComponentsWithType<T>(elementType))
            {
                result.Add(component.Key, component.Value);
            }
        }

        return result;
    }
}