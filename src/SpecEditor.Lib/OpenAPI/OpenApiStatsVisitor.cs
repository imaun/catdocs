using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;

namespace SpecEditor.Lib.OpenAPI;

public class OpenApiStatsVisitor : OpenApiVisitorBase
{
    public int SchemaCount { get; private set; }

    public override void Visit(OpenApiSchema schema)
    {
        SchemaCount++;
    }


    
}
