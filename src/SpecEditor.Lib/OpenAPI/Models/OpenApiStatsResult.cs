namespace SpecEditor.Lib.OpenAPI;

public record OpenApiStatsResult(
    int PathItemsCount, int OperationsCount, int ParametersCount,
    int RequestBodyCount, int ResponsesCount, int LinkesCount,
    int CallbacksCount, int SchemasCount
    );
