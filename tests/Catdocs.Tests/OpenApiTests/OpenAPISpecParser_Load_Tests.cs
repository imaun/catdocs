using Catdocs.Lib.OpenAPI;
using Catdocs.Tests.Common;
using Microsoft.OpenApi;

namespace Catdocs.Tests.OpenApiTests;

public class OpenAPISpecParser_Load_Tests
{
    
    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml, "valid_simple_oas.yaml")]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json, "valid_simple_oas.json")]
    public void load_valid_open_api_document_should_succeed(
        OpenApiSpecVersion version, OpenApiFormat format, string filePath)
    {
        
        var parser = new OpenAPISpecParser(
            TestDataProvider.GetTestDataFilePath(filePath), version, format
            );

        var info = parser.Load();
        
        Assert.NotNull(info);
        Assert.True(info.Success);
        Assert.Empty(info.Errors);
    }
}