using System.Reflection.Metadata;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

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

    public static ReferenceType? GetOpenApiReferenceType(this string elementTypeName)
    {
        return elementTypeName switch
        {
            OpenApiConstants.Schema => ReferenceType.Schema,
            OpenApiConstants.Parameter => ReferenceType.Parameter,
            OpenApiConstants.Callback => ReferenceType.Callback,
            OpenApiConstants.Example => ReferenceType.Example,
            OpenApiConstants.Header => ReferenceType.Header,
            OpenApiConstants.Link => ReferenceType.Link,
            OpenApiConstants.Response => ReferenceType.Response,
            OpenApiConstants.RequestBody => ReferenceType.RequestBody,
            OpenApiConstants.Path => ReferenceType.Path,
            OpenApiConstants.Tag => ReferenceType.Tag,
            OpenApiConstants.SecurityScheme => ReferenceType.SecurityScheme,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static void WriteListToConsole(
        this List<string> list,
        bool useLineNo = false,
        bool useTab = false, 
        ConsoleColor color = ConsoleColor.White)
    {
        if (!list.Any())
        {
            return;
        }
        
        Console.ForegroundColor = color;
        int lineNo = 1;
        foreach (var item in list)
        {
            var output = item;
            if (useLineNo)
            {
                output = $"{lineNo}: {output}";
            }
            
            if (useTab)
            {
                output = $"     {output}";
            }
            
            Console.WriteLine(output);

            lineNo++;
        }
        Console.ResetColor();
    }

    public static void WriteToConsole(this OpenApiStatsResult stats)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(stats);
        Console.ResetColor();
    }

    public static string GetErrorLogForElementType(
        this OpenApiDiagnostic diagnostics, string elementType, string filename = "")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Read type: {elementType} " + filename != string.Empty ? $"from file {filename}" : "");
        if (diagnostics.Errors.Any())
        {
            sb.AppendLine("Errors: ");
            foreach (var err in diagnostics.Errors)
            {
                sb.AppendLine($"     - {err}");
            }
        }

        return sb.ToString();
    }
}