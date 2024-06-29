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


    public int ParametersCount { get; private set; }

    public override void Visit(OpenApiParameter parameter)
    {
        ParametersCount++;
    }


    public int HeaderCount { get; private set; }

    public override void Visit(IDictionary<string, OpenApiHeader> headers)
    {
        HeaderCount++;
    }

    public int PathItemCount { get; private set; }

    public override void Visit(OpenApiPathItem pathItem)
    {
        PathItemCount++;
    }

    
    public int ResponseCount { get; private set; }

    public override void Visit(OpenApiResponses response)
    {
        ResponseCount++;
    }


    public int OperationCount { get; private set; }

    public override void Visit(OpenApiOperation operation)
    {
        OperationCount++;
    }

    public int LinkCount { get; private set; }

    public override void Visit(OpenApiLink operation)
    {
        LinkCount++;
    }

    public int CallbackCount { get; set; }

    public override void Visit(OpenApiCallback callback)
    {
        CallbackCount++;
    }
}
