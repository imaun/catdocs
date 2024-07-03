using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Catdocs.Lib.OpenAPI;

public static class ExportExtensions
{

    public static void ExportPaths(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Paths.Any())
        {
            SpecLogger.Log("No API Paths found!");
            return;
        }

        var paths_dir = Path.Combine(outputDir, OpenApiConstants.Path_Dir);
        CreateDirIfNotExists(paths_dir);

        foreach (var path in document.Paths)
        {
            var filename = Path.Combine(
                paths_dir,
                $"{GetNormalizedPathFilename(path.Key)}.{format.GetFormatFileExtension()}");

            try
            {
                var content = SerializeElement(path.Value, version, format);
                SaveToFile(filename, content);
            }
            catch (Exception ex)
            {
                SpecLogger.Log($"{nameof(ExportPaths)} Exception: {ex.GetBaseException().Message}");
            }
            finally
            {
                SpecLogger.Log($"Exported API Path: {path.Key} to {filename}");
            }
        }

        SpecLogger.Log("Export API Paths finished.");
    }

    public static void ExportSchemas(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Schemas.Any())
        {
            SpecLogger.Log("No Schema found!");
            return;
        }
        
        document.Export(document.Components.Schemas, outputDir, version, format);
    }


    public static void ExportParameters(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Parameters.Any())
        {
            SpecLogger.Log("No Parameters found!");
            return;
        }
        
        document.Export(document.Components.Parameters, outputDir, version, format);
    }

    public static void ExportExamples(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Examples.Any())
        {
            SpecLogger.Log("No Examples found!");
            return;
        }

        document.Export(document.Components.Examples, outputDir, version, format);
    }

    public static void ExportHeaders(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Headers.Any())
        {
            return;
        }

        document.Export(document.Components.Headers, outputDir, version, format);
    }


    public static void ExportResponses(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Responses.Any())
        {
            SpecLogger.Log("No Response found!");
            return;
        }

        document.Export(document.Components.Responses, outputDir, version, format);
    }

    public static void ExportLinks(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Links.Any())
        {
            SpecLogger.Log("No Links found!");
            return;
        }
        
        document.Export(document.Components.Links, outputDir, version, format);
    }

    public static void ExportCallbacks(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Callbacks.Any())
        {
            SpecLogger.Log("No Callbacks found!");
            return;
        }

        document.Export(document.Components.Callbacks, outputDir, version, format);
    }

    public static void ExportRequestBodies(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.RequestBodies.Any())
        {
            SpecLogger.Log("No RequestBody found!");
            return;
        }

        document.Export(document.Components.RequestBodies, outputDir, version, format);
    }

    private static void Export<T>(
        this OpenApiDocument document,
        IDictionary<string, T> elements, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
        where T : IOpenApiReferenceable
    {
        string elementTypeName = GetOpenApiElementTypeName(typeof(T));
        string dir = Path.Combine(outputDir, GetOpenApiElementDirectoryName(typeof(T)));
        
        CreateDirIfNotExists(dir);
        
        //Keep a clone of the current elements in the document
        IDictionary<string, T> elementsClone = elements;
        document.DeleteAllElementsOfType(elementTypeName);
        
        foreach (var el in elementsClone)
        {
            var filename = Path.Combine(dir, $"{el.Key}.{format.GetFormatFileExtension()}");
            try
            {
                var content = SerializeElement(el.Value, version, format);
                SaveToFile(filename, content);
                
                document.AddExternalReferenceFor(elementTypeName, el.Key, filename);
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
        
        SpecLogger.Log($"Export {elementTypeName} finished.");
    }

    private static string GetOpenApiElementTypeName<T>(T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => OpenApiConstants.Schema,
            OpenApiParameter => OpenApiConstants.Parameter,
            OpenApiExample => OpenApiConstants.Example,
            OpenApiHeader => OpenApiConstants.Header,
            OpenApiResponse => OpenApiConstants.Response,
            OpenApiRequestBody => OpenApiConstants.RequestBody,
            OpenApiLink => OpenApiConstants.Link,
            OpenApiCallback => OpenApiConstants.Callback,
            OpenApiSecurityScheme => OpenApiConstants.SecurityScheme,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    private static string GetOpenApiElementTypeName(Type type)
    {
        if (type == typeof(OpenApiSchema))
        {
            return OpenApiConstants.Schema;
        }

        if (type == typeof(OpenApiParameter))
        {
            return OpenApiConstants.Parameter;
        }

        if (type == typeof(OpenApiExample))
        {
            return OpenApiConstants.Example;
        }

        if (type == typeof(OpenApiHeader))
        {
            return OpenApiConstants.Header;
        }

        if (type == typeof(OpenApiResponse))
        {
            return OpenApiConstants.Response;
        }

        if (type == typeof(OpenApiRequestBody))
        {
            return OpenApiConstants.RequestBody;
        }

        if (type == typeof(OpenApiLink))
        {
            return OpenApiConstants.Link;
        }

        if (type == typeof(OpenApiCallback))
        {
            return OpenApiConstants.Callback;
        }

        if (type == typeof(OpenApiSecurityScheme))
        {
            return OpenApiConstants.SecurityScheme;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }

    private static string GetOpenApiElementDirectoryName<T>(T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => OpenApiConstants.Schema_Dir,
            OpenApiParameter => OpenApiConstants.Parameter_Dir,
            OpenApiExample => OpenApiConstants.Example_Dir,
            OpenApiHeader => OpenApiConstants.Header_Dir,
            OpenApiResponse => OpenApiConstants.Response_Dir,
            OpenApiRequestBody => OpenApiConstants.RequestBody_Dir,
            OpenApiLink => OpenApiConstants.Link_Dir,
            OpenApiCallback => OpenApiConstants.Callback_Dir,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    private static string GetOpenApiElementDirectoryName(Type type)
    {
        if (type == typeof(OpenApiSchema))
        {
            return OpenApiConstants.Schema_Dir;
        }

        if (type == typeof(OpenApiParameter))
        {
            return OpenApiConstants.Parameter_Dir;
        }

        if (type == typeof(OpenApiExample))
        {
            return OpenApiConstants.Example_Dir;
        }

        if (type == typeof(OpenApiHeader))
        {
            return OpenApiConstants.Header_Dir;
        }

        if (type == typeof(OpenApiResponse))
        {
            return OpenApiConstants.Response_Dir;
        }

        if (type == typeof(OpenApiRequestBody))
        {
            return OpenApiConstants.RequestBody_Dir;
        }

        if (type == typeof(OpenApiLink))
        {
            return OpenApiConstants.Link_Dir;
        }

        if (type == typeof(OpenApiCallback))
        {
            return OpenApiConstants.Callback_Dir;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }


    private static string SerializeElement<T>(
        T element, OpenApiSpecVersion version, OpenApiFormat format) where T: IOpenApiReferenceable
    {
        using var stream = new MemoryStream();
        element.Serialize(stream, version, format, new OpenApiWriterSettings
        {
            InlineLocalReferences = true,
            InlineExternalReferences = true
        });
        stream.Position = 0;
        
        return new StreamReader(stream).ReadToEnd();
    }
    
    private static void SaveToFile(string filePath, string content)
    {
        var fs = new FileStream(
            filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.Write(content);
        stream_writer.Flush();
        stream_writer.Close();
    }
    
    private static string GetNormalizedPathFilename(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            //TODO: log error
            throw new ArgumentNullException(nameof(path));
        }

        return path.TrimStart('/').Replace('/', '_');
    }
    
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

    private static void DeleteAllElementsOfType(this OpenApiDocument document, string elementTypeName)
    {
        switch (elementTypeName)
        {
            case OpenApiConstants.Schema:
                document.Components.Schemas = new Dictionary<string, OpenApiSchema>();
                break;
            case OpenApiConstants.Parameter:
                document.Components.Parameters = new Dictionary<string, OpenApiParameter>();
                break;
            case OpenApiConstants.Callback:
                document.Components.Callbacks = new Dictionary<string, OpenApiCallback>();
                break;
            case OpenApiConstants.Example:
                document.Components.Examples = new Dictionary<string, OpenApiExample>();
                break;
            case OpenApiConstants.Header:
                document.Components.Headers = new Dictionary<string, OpenApiHeader>();
                break;
            case OpenApiConstants.Link:
                document.Components.Links = new Dictionary<string, OpenApiLink>();
                break;
            case OpenApiConstants.Response:
                document.Components.Responses = new Dictionary<string, OpenApiResponse>();
                break;
            case OpenApiConstants.RequestBody:
                document.Components.RequestBodies = new Dictionary<string, OpenApiRequestBody>();
                break;
        }
    }

    private static void AddExternalReferenceFor(
        this OpenApiDocument document, string elementTypeName, string key, string filePath)
    {
        switch (elementTypeName)
        {
            case OpenApiConstants.Schema:
                document.Components.Schemas.Add(key, new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Parameter:
                document.Components.Parameters.Add(key, new OpenApiParameter
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Callback:
                document.Components.Callbacks.Add(key, new OpenApiCallback
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Example:
                document.Components.Examples.Add(key, new OpenApiExample
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Header:
                document.Components.Headers.Add(key, new OpenApiHeader
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Link:
                document.Components.Links.Add(key, new OpenApiLink
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Response:
                document.Components.Responses.Add(key, new OpenApiResponse
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.RequestBody:
                document.Components.RequestBodies.Add(key, new OpenApiRequestBody
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Path:
                document.Paths.Add(key, new OpenApiPathItem
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
        }
    }
}