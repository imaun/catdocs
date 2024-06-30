using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace SpecEditor.Lib.OpenAPI;

public static class ExportExtensions
{

    public static void ExportSchemas(
        this OpenApiDocument document, 
        string outputDir, 
        OpenApiSpecVersion version, 
        OpenApiFormat format)
    {
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (!document.Components.Schemas.Any())
        {
            return;
        }
        
        var schema_dir = Path.Combine(outputDir, $"schemas");
        CreateDirIfNotExists(schema_dir);
        
        
        foreach (var schema in document.Components.Schemas)
        {
            string filename = Path.Combine(schema_dir, $"{schema.Key}.{format.GetFormatFileExtension()}");
            using var stream = new MemoryStream();
            schema.Value.Serialize(stream, version, format, new OpenApiWriterSettings
            {
                InlineLocalReferences = true,
                InlineExternalReferences = true
            });
            stream.Position = 0;
            
            var content = new StreamReader(stream).ReadToEnd();
            
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
}