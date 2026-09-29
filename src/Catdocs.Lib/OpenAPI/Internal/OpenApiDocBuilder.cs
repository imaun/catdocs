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


    public async Task<OpenApiDocument> BundleAsync(CancellationToken cancellationToken = default)
    {
        var api_paths = new OpenApiPaths();
        foreach (var path in _document.Paths)
        {
            if (path.Value is OpenApiPathItemReference reference)
            {
                var pathRef = reference.Reference.ReferenceV3;
                ArgumentException.ThrowIfNullOrWhiteSpace(pathRef);
                var filePath = Path.Combine(_inputDir, pathRef.Split('#', 2)[0]);
                var pathDoc = await LoadApiPathDocumentAsync(Path.GetFullPath(filePath), cancellationToken)
                    .ConfigureAwait(false);
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

        _document.Components.Schemas = await ResolveReferencesAsync<IOpenApiSchema>(cancellationToken).ConfigureAwait(false);
        _document.Components.Callbacks = await ResolveReferencesAsync<IOpenApiCallback>(cancellationToken).ConfigureAwait(false);
        _document.Components.Examples = await ResolveReferencesAsync<IOpenApiExample>(cancellationToken).ConfigureAwait(false);
        _document.Components.Parameters = await ResolveReferencesAsync<IOpenApiParameter>(cancellationToken).ConfigureAwait(false);
        _document.Components.Headers = await ResolveReferencesAsync<IOpenApiHeader>(cancellationToken).ConfigureAwait(false);
        _document.Components.Responses = await ResolveReferencesAsync<IOpenApiResponse>(cancellationToken).ConfigureAwait(false);
        _document.Components.RequestBodies = await ResolveReferencesAsync<IOpenApiRequestBody>(cancellationToken).ConfigureAwait(false);
        _document.Components.Links = await ResolveReferencesAsync<IOpenApiLink>(cancellationToken).ConfigureAwait(false);

        return _document;
    }


    internal async Task<OpenApiDocument> LoadApiPathDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var file_stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        var result = await OpenApiDocument.LoadAsync(
            file_stream,
            _format.ToStr(),
            OpenApiExtensions.CreateReaderSettings(filePath),
            cancellationToken).ConfigureAwait(false);
        return result.Document
            ?? throw new InvalidDataException($"Unable to parse referenced document '{filePath}'.");
    }

    internal async Task<Dictionary<string, T>> ResolveReferencesAsync<T>(CancellationToken cancellationToken = default) 
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
            var componentPart = await LoadApiPathDocumentAsync(f, cancellationToken).ConfigureAwait(false);
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