using Catdocs.Lib.OpenAPI;
using Catdocs.Tests.Common;
using Microsoft.OpenApi;

namespace Catdocs.Tests.OpenApiTests;

public class OpenAPISpecParser_Load_Tests
{
    
    [Fact]
    public void load_valid_open_api_document_should_succeed()
    {
        var validFilePath = "valid_simple_oas.yaml";

        var parser = new OpenAPISpecParser(
            TestDataProvider.GetTestDataFilePath(validFilePath)
            );

        var info = parser.Load();
        
        Assert.NotNull(info);
        Assert.True(info.Success);
        Assert.Empty(info.Errors);
    }
}