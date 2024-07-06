using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocBuilder
{
    private readonly OpenApiDocument _document;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private string _inputDir;


    public OpenApiDocBuilder(OpenApiDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        
        
    }


    internal string GetDocumentFilenameFromPath(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return inputPath;
        }

        if (Directory.Exists(inputPath))
        {
            var file = Directory
                .EnumerateFiles(inputPath, "*.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                     f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));

            if (file is not null)
            {
                return file;
            }
        }
        
        string err = $"No OpenAPI files found in: {inputPath}";
        SpecLogger.LogError(err);
        throw new FileNotFoundException(err); 
    }
}