using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

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
        
        var schema_dir = Path.Combine(outputDir, $".{Path.PathSeparator}schemas");
        CreateDirIfNotExists(schema_dir);
        
        foreach (var schema in document.Components.Schemas)
        {
            string filename = Path.Combine(schema_dir, $"{schema.Key}.{format.GetFormatFileExtension()}");
            var stream = new MemoryStream();
            schema.Value.Serialize(stream, version, format);
            stream.Position = 0;
        }
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