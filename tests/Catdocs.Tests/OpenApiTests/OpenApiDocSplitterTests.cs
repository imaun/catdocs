using Catdocs.Lib.OpenAPI.Internal;
using Microsoft.OpenApi.Models;

namespace Catdocs.Tests.OpenApiTests;

public class OpenApiDocSplitterTests
{
    
    [Theory]
    [InlineData(@"C:\example\output\paths\get_by_id.yaml", @"C:\example\output", @"paths\get_by_id.yaml")]
    [InlineData("~/example/output/paths/get_by_id.yaml", "~/example/output", @"paths\get_by_id.yaml")]
    public void GetRelativePath_Should_Return_Correct_RelativePath(
        string inputFilePath, string outputDir, string expectedRelativePath)
    {
        var document = new OpenApiDocument();
        var splitter = new OpenApiDocSplitter(outputDir, document);

        string actualRelativePath = splitter.GetRelativePath(inputFilePath);

        Assert.Equal(expectedRelativePath, actualRelativePath);
    }
}