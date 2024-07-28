using Catdocs.Lib.OpenAPI.Internal;
using Microsoft.OpenApi.Models;
using Catdocs.Tests.Common;

namespace Catdocs.Tests.OpenApiTests;

public class OpenApiDocSplitterTests
{
    
    [Theory]
    [MemberData(nameof(OpenApiTestDataProvider.GetRelativePathTestData), MemberType = typeof(OpenApiTestDataProvider))]
    public void GetRelativePath_Should_Return_Correct_RelativePath(
        string inputFilePath, string outputDir, string expectedRelativePath)
    {
        var document = new OpenApiDocument();
        var splitter = new OpenApiDocSplitter(outputDir, document);

        string actualRelativePath = splitter.GetRelativePath(inputFilePath);

        Assert.Equal(expectedRelativePath, actualRelativePath);
    }
    
    public void split_simple_oas()
    {
    }
}