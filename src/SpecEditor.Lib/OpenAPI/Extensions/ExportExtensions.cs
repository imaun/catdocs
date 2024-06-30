using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace SpecEditor.Lib.OpenAPI;

public static class ExportExtensions
{

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
    
    private static void SaveToFile(string filePath, string content)
    {
        var fs = new FileStream(
            filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.Write(content);
        stream_writer.Flush();
        stream_writer.Close();
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