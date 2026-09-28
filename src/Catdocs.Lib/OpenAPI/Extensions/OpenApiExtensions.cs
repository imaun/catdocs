using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Catdocs.Lib.OpenAPI.Extensions;

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
            return OpenApiConstants.Json;

        if (format is OpenApiFormat.Yaml)
            return OpenApiConstants.Yaml;

        return "Unknown";
    }

    public static string GetFormatFileExtension(this OpenApiFormat format)
    {
        if (format is OpenApiFormat.Json)
            return OpenApiConstants.Json;

        if (format is OpenApiFormat.Yaml)
            return OpenApiConstants.Yaml;

        return "txt";
    }

    public static ReferenceType? GetOpenApiReferenceType(this string elementTypeName)
    {
        return elementTypeName switch
        {
            Constants.Schema => ReferenceType.Schema,
            Constants.Parameter => ReferenceType.Parameter,
            Constants.Callback => ReferenceType.Callback,
            Constants.Example => ReferenceType.Example,
            Constants.Header => ReferenceType.Header,
            Constants.Link => ReferenceType.Link,
            Constants.Response => ReferenceType.Response,
            Constants.RequestBody => ReferenceType.RequestBody,
            Constants.Path => ReferenceType.PathItem,
            Constants.Tag => ReferenceType.Tag,
            Constants.SecurityScheme => ReferenceType.SecurityScheme,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static OpenApiReaderSettings CreateReaderSettings(string filePath)
    {
        var settings = new OpenApiReaderSettings
        {
            BaseUrl = new Uri(Path.GetFullPath(filePath))
        };
        settings.AddYamlReader();
        return settings;
    }

    public static string? GetDeclaredSpecVersion(string content, OpenApiFormat format)
    {
        if (format == OpenApiFormat.Json)
        {
            try
            {
                using var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("openapi", out var openApi))
                {
                    return openApi.GetString();
                }

                if (json.RootElement.TryGetProperty("swagger", out var swagger))
                {
                    return swagger.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }

            return null;
        }

        foreach (var line in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("openapi:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("swagger:", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[(trimmed.IndexOf(':') + 1)..].Trim().Trim('\'', '"');
            }
        }

        return null;
    }

    public static string PreserveDeclaredSpecVersion(
        this string content,
        string? declaredVersion,
        OpenApiFormat format)
    {
        if (string.IsNullOrWhiteSpace(declaredVersion))
        {
            return content;
        }

        var pattern = format == OpenApiFormat.Json
            ? "(\"(?:openapi|swagger)\"\\s*:\\s*\")[^\"]+(\"\\s*)"
            : "(?m)^(\\s*(?:openapi|swagger)\\s*:\\s*)(['\"]?)[^'\"\\r\\n]+(['\"]?\\s*)$";
        return Regex.Replace(
            content,
            pattern,
            match => format == OpenApiFormat.Json
                ? $"{match.Groups[1].Value}{declaredVersion}{match.Groups[2].Value}"
                : $"{match.Groups[1].Value}{match.Groups[2].Value}{declaredVersion}{match.Groups[3].Value}",
            RegexOptions.None,
            TimeSpan.FromSeconds(1));
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