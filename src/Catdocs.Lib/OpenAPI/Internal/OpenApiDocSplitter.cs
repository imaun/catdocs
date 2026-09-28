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


    public void Split()
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
                temp_document.SaveDocumentToFile(
                    _version,
                    _format,
                    filename,
                    _declaredVersion);

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
                SpecLogger.Log($"{nameof(Split)} Exception: {ex.GetBaseException().Message}");
            }
            finally
            {
                SpecLogger.Log($"Exported API Path: {path.Key} to {filename}");
            }
        }
        
        SpecLogger.Log("Export API Paths finished.");
        
        ExportComponents();
        
        var documentFilename = $"{_outputDir}{Path.DirectorySeparatorChar}OpenApi.{_format.GetFormatFileExtension()}";
        var documentContent = _document
            .SerializeDocumentAsync(_version, _format)
            .GetAwaiter()
            .GetResult()
            .PreserveDeclaredSpecVersion(_declaredVersion, _format);
        foreach (var replacement in _pathReferenceReplacements)
        {
            documentContent = documentContent.Replace(
                replacement.Key,
                replacement.Value,
                StringComparison.Ordinal);
        }
        File.WriteAllText(documentFilename, documentContent);
        SpecLogger.Log($"Main document created at : {documentFilename}");
    }
    
    private void ExportComponents()
    {
        ExportSchemas();
        ExportCallbacks();
        ExportParameters();
        ExportHeaders();
        ExportLinks();
        ExportResponses();
        ExportRequestBodies();
        ExportExamples();
        //ExportSecuritySchemes();
    }

    private void ExportSchemas()
    {
        if (_document.Components?.Schemas?.Any() != true)
        {
            SpecLogger.Log("No Schema found!");
            return;
        }
        
        Export(_document.Components.Schemas);
    }

    private void ExportParameters()
    {
        if (_document.Components?.Parameters?.Any() != true)
        {
            SpecLogger.Log("No Parameters found!");
            return;
        }
        
        Export(_document.Components.Parameters);
    }

    private void ExportExamples()
    {
        if (_document.Components?.Examples?.Any() != true)
        {
            SpecLogger.Log("No Examples found!");
            return;
        }
        
        Export(_document.Components.Examples);
    }

    private void ExportSecuritySchemes()
    {
        if (_document.Components?.SecuritySchemes?.Any() != true)
        {
            SpecLogger.Log("No SecuritySchemes found!");
            return;
        }
        
        Export(_document.Components.SecuritySchemes);
    }

    private void ExportHeaders()
    {
        if (_document.Components?.Headers?.Any() != true)
        {
            SpecLogger.Log("No Headers found!");
            return;
        }
        
        Export(_document.Components.Headers);
    }

    private void ExportResponses()
    {
        if (_document.Components?.Responses?.Any() != true)
        {
            SpecLogger.Log("No Response found!");
            return;
        }
        
        Export(_document.Components.Responses);
    }

    private void ExportLinks()
    {
        if (_document.Components?.Links?.Any() != true)
        {
            SpecLogger.Log("No Links found!");
            return;
        }
        
        Export(_document.Components.Links);
    }

    private void ExportCallbacks()
    {
        if (_document.Components?.Callbacks?.Any() != true)
        {
            SpecLogger.Log("No Callbacks found!");
            return;
        }
        
        Export(_document.Components.Callbacks);
    }

    private void ExportRequestBodies()
    {
        if (_document.Components?.RequestBodies?.Any() != true)
        {
            SpecLogger.Log("No RequestBody found!");
            return;
        }
        
        Export(_document.Components.RequestBodies);
    }
    
    private void Export<T>(IDictionary<string, T> elements) where T : IOpenApiReferenceable
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

                temp_document.SaveDocumentToFile(
                    _version,
                    _format,
                    filename,
                    _declaredVersion);
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