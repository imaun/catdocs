using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocBuilder
{
    private string _inputPath;
    private string _documentFilename;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private readonly OpenApiDocument _document;
    private string _inputDir;


    public OpenApiDocBuilder(string inputPath)
    {
        _documentFilename = GetDocumentFilenameFromPath(inputPath);
        
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