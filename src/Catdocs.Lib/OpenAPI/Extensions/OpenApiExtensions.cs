using Microsoft.OpenApi;

namespace Catdocs.Lib.OpenAPI;

public static class OpenApiExtensions
{

    public static string ToStr(this OpenApiSpecVersion version)
    {
        if (version is OpenApiSpecVersion.OpenApi2_0)
            return "2.0";

        if (version is OpenApiSpecVersion.OpenApi3_0)
            return "3.0";

        return "Unknown";
    }

    public static bool IsJson(this OpenApiFormat format)
        => format is OpenApiFormat.Json;

    public static bool IsYaml(this OpenApiFormat format)
        => format is OpenApiFormat.Yaml;

    public static string ToStr(this OpenApiFormat format)
    {
        if (format is OpenApiFormat.Json)
            return "JSON";

        if (format is OpenApiFormat.Yaml)
            return "YAML";

        return "Unknown";
    }

    public static string GetFormatFileExtension(this OpenApiFormat format)
    {
        if (format is OpenApiFormat.Json)
            return "json";

        if (format is OpenApiFormat.Yaml)
            return "yaml";

        return "txt";
    }

    public static void WriteListToConsole(this List<string> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }

    public static void WriteToConsole(this OpenApiStatsResult stats)
    {
        Console.WriteLine(stats);
    }
}