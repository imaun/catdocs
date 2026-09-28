using Microsoft.OpenApi;

namespace Catdocs.Lib.OpenAPI;

public class OpenApiStatsVisitor : OpenApiVisitorBase
{

    public OpenApiStatsResult GetStats()
    {
        return new OpenApiStatsResult(
            this.PathItemsCount, this.OperationsCount, this.ParametersCount,
            this.RequestBodyCount, this.ResponsesCount, this.LinksCount,
            this.CallbacksCount, this.SchemasCount);
    }

    public int SchemasCount { get; private set; }

    public override void Visit(IOpenApiSchema schema)
    {
        SchemasCount++;
    }


    public int ParametersCount { get; private set; }

    public override void Visit(IOpenApiParameter parameter)
    {
        ParametersCount++;
    }


    public int HeaderCount { get; private set; }

    public override void Visit(IDictionary<string, IOpenApiHeader> headers)
    {
        HeaderCount++;
    }

    public int PathItemsCount { get; private set; }

    public override void Visit(IOpenApiPathItem pathItem)
    {
        PathItemsCount++;
    }

    
    public int ResponsesCount { get; private set; }

    public override void Visit(OpenApiResponses response)
    {
        ResponsesCount++;
    }


    public int OperationsCount { get; private set; }

    public override void Visit(OpenApiOperation operation)
    {
        OperationsCount++;
    }

    public int LinksCount { get; private set; }

    public override void Visit(IOpenApiLink operation)
    {
        LinksCount++;
    }

    public int CallbacksCount { get; set; }

    public override void Visit(IOpenApiCallback callback)
    {
        CallbacksCount++;
    }

    public int RequestBodyCount { get; set; }

    public override void Visit(IOpenApiRequestBody requestBody)
    {
        RequestBodyCount++;
    }
}
