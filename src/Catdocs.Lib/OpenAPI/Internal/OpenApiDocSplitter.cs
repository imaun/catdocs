using Microsoft.OpenApi;
using Catdocs.Lib.OpenAPI.Extensions;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocSplitter
{
    private OpenApiDocument _document;
    private string _outputDir;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private readonly Dictionary<string, string> _pathReferenceReplacements = [];
    private readonly string? _declaredVersion;

    public OpenApiDocSplitter(
        string outputDir,
        OpenApiDocument document,
        OpenApiSpecVersion version = OpenApiSpecVersion.OpenApi3_0,
        OpenApiFormat format = OpenApiFormat.Yaml,
        string? declaredVersion = null
        )
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));

        _document = document;
        _version = version;
        _format = format;
        _declaredVersion = declaredVersion;

        _outputDir = outputDir;
        CreateDirIfNotExists(_outputDir);
    }


    public async Task SplitAsync(CancellationToken cancellationToken = default)
    {
        if (!_document.Paths.Any())
        {
            SpecLogger.Log("No API Paths found!");
            return;
        }
        
        var paths_dir = Path.Combine(_outputDir, Constants.Path_Dir);
        CreateDirIfNotExists(paths_dir);
        
        var paths = _document.Paths;
        _document.Paths = new OpenApiPaths();
        
        foreach (var path in paths)
        {
            var filename = Path.Combine(
                paths_dir,
                $"{GetNormalizedOpenApiPathFilename(path.Key)}.{_format.GetFormatFileExtension()}");

            try
            {
                var temp_document = new OpenApiDocument
                {
                    Paths = new OpenApiPaths()
                };
                
                temp_document.Paths.Add(path.Key, path.Value);
                await temp_document.SaveDocumentToFileAsync(
                    _version,
                    _format,
                    filename,
                    _declaredVersion,
                    cancellationToken).ConfigureAwait(false);

                var referenceId = GetNormalizedOpenApiPathFilename(path.Key);
                var relativePath = GetRelativePath(filename);
                var pathReference = new OpenApiPathItemReference(
                    referenceId,
                    _document,
                    relativePath);
                _pathReferenceReplacements[
                    $"{relativePath}#/components/pathItems/{referenceId}"] =
                    $"{relativePath}#/paths/{EscapeJsonPointer(path.Key)}";
                _document.Paths.Add(path.Key, pathReference);
            }
            catch (Exception ex)
            {
                SpecLogger.Log($"{nameof(SplitAsync)} Exception: {ex.GetBaseException().Message}");
            }
            finally
            {
                SpecLogger.Log($"Exported API Path: {path.Key} to {filename}");
            }
        }
        
        SpecLogger.Log("Export API Paths finished.");
        
        await ExportComponentsAsync(cancellationToken).ConfigureAwait(false);
        
        var documentFilename = $"{_outputDir}{Path.DirectorySeparatorChar}OpenApi.{_format.GetFormatFileExtension()}";
        var documentContent = (await _document
                .SerializeDocumentAsync(_version, _format, cancellationToken)
                .ConfigureAwait(false))
            .PreserveDeclaredSpecVersion(_declaredVersion, _format);
        foreach (var replacement in _pathReferenceReplacements)
        {
            documentContent = documentContent.Replace(
                replacement.Key,
                replacement.Value,
                StringComparison.Ordinal);
        }
        await File.WriteAllTextAsync(documentFilename, documentContent, cancellationToken).ConfigureAwait(false);
        SpecLogger.Log($"Main document created at : {documentFilename}");
    }
    
    private async Task ExportComponentsAsync(CancellationToken cancellationToken)
    {
        await ExportSchemasAsync(cancellationToken).ConfigureAwait(false);
        await ExportCallbacksAsync(cancellationToken).ConfigureAwait(false);
        await ExportParametersAsync(cancellationToken).ConfigureAwait(false);
        await ExportHeadersAsync(cancellationToken).ConfigureAwait(false);
        await ExportLinksAsync(cancellationToken).ConfigureAwait(false);
        await ExportResponsesAsync(cancellationToken).ConfigureAwait(false);
        await ExportRequestBodiesAsync(cancellationToken).ConfigureAwait(false);
        await ExportExamplesAsync(cancellationToken).ConfigureAwait(false);
        //ExportSecuritySchemes();
    }

    private Task ExportSchemasAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Schemas?.Any() != true)
        {
            SpecLogger.Log("No Schema found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Schemas, cancellationToken);
    }

    private Task ExportParametersAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Parameters?.Any() != true)
        {
            SpecLogger.Log("No Parameters found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Parameters, cancellationToken);
    }

    private Task ExportExamplesAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Examples?.Any() != true)
        {
            SpecLogger.Log("No Examples found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Examples, cancellationToken);
    }

    private Task ExportSecuritySchemesAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.SecuritySchemes?.Any() != true)
        {
            SpecLogger.Log("No SecuritySchemes found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.SecuritySchemes, cancellationToken);
    }

    private Task ExportHeadersAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Headers?.Any() != true)
        {
            SpecLogger.Log("No Headers found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Headers, cancellationToken);
    }

    private Task ExportResponsesAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Responses?.Any() != true)
        {
            SpecLogger.Log("No Response found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Responses, cancellationToken);
    }

    private Task ExportLinksAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Links?.Any() != true)
        {
            SpecLogger.Log("No Links found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Links, cancellationToken);
    }

    private Task ExportCallbacksAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.Callbacks?.Any() != true)
        {
            SpecLogger.Log("No Callbacks found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.Callbacks, cancellationToken);
    }

    private Task ExportRequestBodiesAsync(CancellationToken cancellationToken)
    {
        if (_document.Components?.RequestBodies?.Any() != true)
        {
            SpecLogger.Log("No RequestBody found!");
            return Task.CompletedTask;
        }
        
        return ExportAsync(_document.Components.RequestBodies, cancellationToken);
    }
    
    private async Task ExportAsync<T>(IDictionary<string, T> elements, CancellationToken cancellationToken) where T : IOpenApiReferenceable
    {
        string elementTypeName = typeof(T).GetOpenApiElementTypeName();
        string dir = Path.Combine(_outputDir, typeof(T).GetOpenApiElementDirectoryName());
        var exportedElements = new List<(string Key, string FilePath)>();
        
        CreateDirIfNotExists(dir);
        
        foreach (var el in elements)
        {
            var filename = Path.Combine(dir, $"{el.Key}.{_format.GetFormatFileExtension()}");
            
            try
            {
                var temp_document = new OpenApiDocument
                {
                    Components = new OpenApiComponents()
                };

                if (!temp_document.AddComponent(el.Key, el.Value))
                {
                    throw new InvalidOperationException(
                        $"Unable to add {elementTypeName} component '{el.Key}'.");
                }

                await temp_document.SaveDocumentToFileAsync(
                    _version,
                    _format,
                    filename,
                    _declaredVersion,
                    cancellationToken).ConfigureAwait(false);
                exportedElements.Add((el.Key, filename));
            }
            catch (Exception ex)
            {
                SpecLogger.LogException(elementTypeName, ex);
            }
            finally
            {
                SpecLogger.Log($"Exported {elementTypeName}: {el.Key} to {filename}");
            }
        }
        
        var components = _document.Components
            ?? throw new InvalidOperationException("The OpenAPI document has no components collection.");
        components.DeleteAllElementsOfType(elementTypeName);

        foreach (var exportedElement in exportedElements)
        {
            components.AddExternalReferenceFor(
                elementTypeName,
                exportedElement.Key,
                GetRelativePath(exportedElement.FilePath));
        }
        
        SpecLogger.Log($"Export {elementTypeName} finished.");
    }

    private static string GetNormalizedOpenApiPathFilename(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            //TODO: log error
            throw new ArgumentNullException(nameof(path));
        }

        return path.TrimStart('/').Replace('/', '_');
    }

    private static string EscapeJsonPointer(string value)
        => value.Replace("~", "~0").Replace("/", "~1");

    private static void CreateDirIfNotExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    internal string GetRelativePath(string filename)
    {
        var fullFilePath = Path.GetFullPath(filename);
        var fullBasePath = Path.GetFullPath(_outputDir);

        var filePathUri = new Uri(fullFilePath);
        var basePathUri = new Uri(fullBasePath + Path.DirectorySeparatorChar);

        var relativeUri = basePathUri.MakeRelativeUri(filePathUri);

        // var result = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        var result = Uri.UnescapeDataString(relativeUri.ToString());
        return result;
    }
}