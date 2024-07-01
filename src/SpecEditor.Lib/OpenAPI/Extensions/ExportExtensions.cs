﻿using Microsoft.OpenApi;
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
            SpecLogger.Log("No API Paths found!");
            return;
        }

        var paths_dir = Path.Combine(outputDir, "paths");
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
        
        var schema_dir = Path.Combine(outputDir, $"schemas");
        CreateDirIfNotExists(schema_dir);
        
        foreach (var schema in document.Components.Schemas)
        {
            var filename = Path.Combine(schema_dir, $"{schema.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(schema.Value, version, format);
            
            SpecLogger.Log($"Exported Schema: {schema.Key} to {filename}");
            SaveToFile(filename, content);
        }
        SpecLogger.Log("Export Schemas finished.");
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

        var parameters_dir = Path.Combine(outputDir, "parameters");
        CreateDirIfNotExists(parameters_dir);

        foreach (var param in document.Components.Parameters)
        {
            var filename = Path.Combine(parameters_dir, $"{param.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(param.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported Parameter: {param.Key} to {filename}");
        }
        SpecLogger.Log("Export Parameters finished.");
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

        var examples_dir = Path.Combine(outputDir, "examples");
        CreateDirIfNotExists(examples_dir);

        foreach (var example in document.Components.Examples)
        {
            var filename = Path.Combine(examples_dir, $"{example.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(example.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported Example: {example.Key} to {filename}");
        }
        SpecLogger.Log("Export Examples finished.");
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
            SpecLogger.Log("No Response found!");
            return;
        }

        var responses_dir = Path.Combine(outputDir, "responses");
        CreateDirIfNotExists(responses_dir);

        foreach (var resp in document.Components.Responses)
        {
            var filename = Path.Combine(responses_dir, $"{resp.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(resp.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported Response: {resp.Key} to {filename}");
        }
        SpecLogger.Log("Export Response finished.");
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

        var links_dir = Path.Combine(outputDir, "links");
        CreateDirIfNotExists(links_dir);

        foreach (var link in document.Components.Links)
        {
            var filename = Path.Combine(links_dir, $"{link.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(link.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported Link: {link.Key} to {filename}");
        }
        SpecLogger.Log("Export Links finished.");
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

        var callbacks_dir = Path.Combine(outputDir, "callbacks");
        CreateDirIfNotExists(callbacks_dir);

        foreach (var callback in document.Components.Callbacks)
        {
            var filename = Path.Combine(callbacks_dir, $"{callback.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(callback.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported Callback: {callback.Key} to {filename}");
        }
        SpecLogger.Log("Export Callbacks finished.");
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

        var requests_dir = Path.Combine(outputDir, "requestbodies");
        CreateDirIfNotExists(requests_dir);

        foreach (var requestBody in document.Components.RequestBodies)
        {
            var filename = Path.Combine(requests_dir, $"{requestBody.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(requestBody.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported RequestBody: {requestBody.Key} to {filename}");
        }
        SpecLogger.Log("Export RequestBodies finished.");
    }

    public static void ExportSecuritySchemes(
        this OpenApiDocument document, string outputDir, OpenApiSpecVersion version, OpenApiFormat format)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!document.Components.SecuritySchemes.Any())
        {
            SpecLogger.Log("No SecurityScheme found!");
            return;
        }

        var security_schemes_dir = Path.Combine(outputDir, "securityschemes");
        CreateDirIfNotExists(security_schemes_dir);
        
        foreach (var securityScheme in document.Components.SecuritySchemes)
        {
            var filename = Path.Combine(security_schemes_dir, $"{securityScheme.Key}.{format.GetFormatFileExtension()}");
            var content = SerializeElement(securityScheme.Value, version, format);
            
            SaveToFile(filename, content);
            SpecLogger.Log($"Exported SecurityScheme: {securityScheme.Key} to {filename}");
        }
        SpecLogger.Log("Export SecurityScheme finished.");
    }

    private static void Export<T>(
        T element, string outputDir, OpenApiSpecVersion version, OpenApiFormat format) where T : IOpenApiReferenceable
    {
        string elementTypeName = "";
        string dir = "";
        if (element is OpenApiSchema)
        {
            elementTypeName = OpenApiConstants.Schema;
            dir = OpenApiConstants.Schema_Dir;
        }

        if (element is OpenApiParameter)
        {
            elementTypeName = OpenApiConstants.Parameter;
            dir = OpenApiConstants.Parameter_Dir;
        }

        if (element is OpenApiExample)
        {
            elementTypeName = OpenApiConstants.Example;
            dir = OpenApiConstants.Example_Dir;
        }

        if (element is OpenApiHeader)
        {
            elementTypeName = OpenApiConstants.Header;
            dir = OpenApiConstants.Header_Dir;
        }

        if (element is OpenApiResponse)
        {
            elementTypeName = OpenApiConstants.Response;
            dir = OpenApiConstants.Response_Dir;
        }

        if (element is OpenApiRequestBody)
        {
            elementTypeName = OpenApiConstants.RequestBody;
            dir = OpenApiConstants.RequestBody_Dir;
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

}