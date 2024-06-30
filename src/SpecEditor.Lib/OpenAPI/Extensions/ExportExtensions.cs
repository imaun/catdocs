using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace SpecEditor.Lib.OpenAPI;

public static class ExportExtensions
{

    public static void ExportPaths(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Paths.Any())
        {
            return;
        }

        var paths_dir = Path.Combine(outputDir, "paths");
        CreateDirIfNotExists(paths_dir);

        foreach (var path in document.Paths)
        {
            var filename = Path.Combine(
                paths_dir, 
                $"{GetNormalizedPathFilename(path.Key)}.{format.GetFormatFileExtension()}");
            
            var content = SerializeElement(path.Value, version, format);
            SaveToFile(filename, content);
        }
    }
    
    public static void ExportSchemas(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Schemas.Any())
        {
            return;
        }
        
        var schema_dir = Path.Combine(outputDir, $"schemas");
        CreateDirIfNotExists(schema_dir);
        
        foreach (var schema in document.Components.Schemas)
        {
            var filename = Path.Combine(schema_dir, $"{schema.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(schema.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }


    public static void ExportParameters(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Parameters.Any())
        {
            return;
        }

        var parameters_dir = Path.Combine(outputDir, "parameters");
        CreateDirIfNotExists(parameters_dir);

        foreach (var param in document.Components.Parameters)
        {
            var filename = Path.Combine(parameters_dir, $"{param.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(param.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }
    
    public static void ExportExamples(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Examples.Any())
        {
            return;
        }

        var examples_dir = Path.Combine(outputDir, "examples");
        CreateDirIfNotExists(examples_dir);

        foreach (var example in document.Components.Examples)
        {
            var filename = Path.Combine(examples_dir, $"{example.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(example.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }

    public static void ExportHeaders(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Headers.Any())
        {
            return;
        }

        var headers_dir = Path.Combine(outputDir, "headers");
        CreateDirIfNotExists(headers_dir);

        foreach (var header in document.Components.Headers)
        {
            var filename = Path.Combine(headers_dir, $"{header.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(header.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }


    public static void ExportResponses(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Responses.Any())
        {
            return;
        }

        var responses_dir = Path.Combine(outputDir, "responses");
        CreateDirIfNotExists(responses_dir);

        foreach (var resp in document.Components.Responses)
        {
            var filename = Path.Combine(responses_dir, $"{resp.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(resp.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }

    public static void ExportLinks(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Links.Any())
        {
            return;
        }

        var links_dir = Path.Combine(outputDir, "links");
        CreateDirIfNotExists(links_dir);

        foreach (var link in document.Components.Links)
        {
            var filename = Path.Combine(links_dir, $"{link.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(link.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }

    public static void ExportCallbacks(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.Callbacks.Any())
        {
            return;
        }

        var callbacks_dir = Path.Combine(outputDir, "callbacks");
        CreateDirIfNotExists(callbacks_dir);

        foreach (var callback in document.Components.Callbacks)
        {
            var filename = Path.Combine(callbacks_dir, $"{callback.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(callback.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }

    public static void ExportRequestBodies(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.RequestBodies.Any())
        {
            return;
        }

        var requests_dir = Path.Combine(outputDir, "requestbodies");
        CreateDirIfNotExists(requests_dir);

        foreach (var requestBody in document.Components.RequestBodies)
        {
            var filename = Path.Combine(requests_dir, $"{requestBody.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(requestBody.Value, version, format);
            
            SaveToFile(filename, content);
        }
    }

    public static void ExportSecuritySchemes(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.SecuritySchemes.Any())
        {
            return;
        }

        var security_schemas_dir = Path.Combine(outputDir, "securityschemas");
        CreateDirIfNotExists(security_schemas_dir);
        
        foreach (var securitySchema in document.Components.SecuritySchemes)
        {
            var filename = Path.Combine(security_schemas_dir, $"{securitySchema.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(securitySchema.Value, version, format);
            
            SaveToFile(filename, content);
        }
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
}