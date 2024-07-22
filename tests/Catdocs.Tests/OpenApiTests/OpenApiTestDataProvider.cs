using System.Runtime.InteropServices;

namespace Catdocs.Tests.Common;

public static class OpenApiTestDataProvider
{

    public static TheoryData<string, string, string> GetRelativePathTestData()
    {
        var data = new TheoryData<string, string, string>();

        var dirSep = Path.DirectorySeparatorChar;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            data.Add(@"C:\example\output\paths\get_by_id.yaml", @"C:\example\output", @"paths\get_by_id.yaml");
            data.Add(@"C:\example\main\responses\result_model.json", @"C:\example\main", @"responses\result_model.json");
            data.Add(@"C:\example\main\requestbodies\req_model.json", @"C:\example\main", @"requestbodies\req_model.json");
        }
        else
        {
            data.Add("~/example/output/paths/get_by_id.yaml", "~/example/output", "paths/get_by_id.yaml");
            data.Add("~/example/main/responses/result_model.json", "~/example/main", "responses/result_model.json");
            data.Add("~/example/main/requestbodies/req_model.json", "~/example/main", "requestbodies/req_model.json");
        }

        return data;
    }
}