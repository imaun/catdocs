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


    
}