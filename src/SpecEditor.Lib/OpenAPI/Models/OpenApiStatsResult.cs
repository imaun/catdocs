using System.Text;

namespace SpecEditor.Lib.OpenAPI;

public record OpenApiStatsResult(
    int PathItemsCount, int OperationsCount, int ParametersCount,
    int RequestBodyCount, int ResponsesCount, int LinkesCount,
    int CallbacksCount, int SchemasCount
    )
{

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"PathItems : {PathItemsCount}")
            .AppendLine($"Operations : {OperationsCount}")
            .AppendLine($"Parameters : {ParametersCount}")
            .AppendLine($"RequestBodies : {RequestBodyCount}")
            .AppendLine($"Responses : {ResponsesCount}")
            .AppendLine($"Links : {LinkesCount}")
            .AppendLine($"Callbacks : {CallbacksCount}")
            .AppendLine($"Schemas : {SchemasCount}");

        return sb.ToString();
    }
}
