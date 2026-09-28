using Microsoft.OpenApi;

namespace Catdocs.Lib.OpenAPI.Extensions;

public static class ExportExtensions
{

    public static string GetOpenApiElementTypeName<T>(T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => Constants.Schema,
            OpenApiParameter => Constants.Parameter,
            OpenApiExample => Constants.Example,
            OpenApiHeader => Constants.Header,
            OpenApiResponse => Constants.Response,
            OpenApiRequestBody => Constants.RequestBody,
            OpenApiLink => Constants.Link,
            OpenApiCallback => Constants.Callback,
            OpenApiSecurityScheme => Constants.SecurityScheme,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static string GetOpenApiElementTypeName(this Type type)
    {
        if (type == typeof(OpenApiSchema) || type == typeof(IOpenApiSchema))
        {
            return Constants.Schema;
        }

        if (type == typeof(OpenApiParameter) || type == typeof(IOpenApiParameter))
        {
            return Constants.Parameter;
        }

        if (type == typeof(OpenApiExample) || type == typeof(IOpenApiExample))
        {
            return Constants.Example;
        }

        if (type == typeof(OpenApiHeader) || type == typeof(IOpenApiHeader))
        {
            return Constants.Header;
        }

        if (type == typeof(OpenApiResponse) || type == typeof(IOpenApiResponse))
        {
            return Constants.Response;
        }

        if (type == typeof(OpenApiRequestBody) || type == typeof(IOpenApiRequestBody))
        {
            return Constants.RequestBody;
        }

        if (type == typeof(OpenApiLink) || type == typeof(IOpenApiLink))
        {
            return Constants.Link;
        }

        if (type == typeof(OpenApiCallback) || type == typeof(IOpenApiCallback))
        {
            return Constants.Callback;
        }

        if (type == typeof(OpenApiSecurityScheme) || type == typeof(IOpenApiSecurityScheme))
        {
            return Constants.SecurityScheme;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }

    private static string GetOpenApiElementDirectoryName<T>(this T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => Constants.Schema_Dir,
            OpenApiParameter => Constants.Parameter_Dir,
            OpenApiExample => Constants.Example_Dir,
            OpenApiHeader => Constants.Header_Dir,
            OpenApiResponse => Constants.Response_Dir,
            OpenApiRequestBody => Constants.RequestBody_Dir,
            OpenApiLink => Constants.Link_Dir,
            OpenApiCallback => Constants.Callback_Dir,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static string GetOpenApiElementDirectoryName(this Type type)
    {
        if (type == typeof(OpenApiSchema) || type == typeof(IOpenApiSchema))
        {
            return Constants.Schema_Dir;
        }

        if (type == typeof(OpenApiParameter) || type == typeof(IOpenApiParameter))
        {
            return Constants.Parameter_Dir;
        }

        if (type == typeof(OpenApiExample) || type == typeof(IOpenApiExample))
        {
            return Constants.Example_Dir;
        }

        if (type == typeof(OpenApiHeader) || type == typeof(IOpenApiHeader))
        {
            return Constants.Header_Dir;
        }

        if (type == typeof(OpenApiResponse) || type == typeof(IOpenApiResponse))
        {
            return Constants.Response_Dir;
        }

        if (type == typeof(OpenApiRequestBody) || type == typeof(IOpenApiRequestBody))
        {
            return Constants.RequestBody_Dir;
        }

        if (type == typeof(OpenApiLink) || type == typeof(IOpenApiLink))
        {
            return Constants.Link_Dir;
        }

        if (type == typeof(OpenApiCallback) || type == typeof(IOpenApiCallback))
        {
            return Constants.Callback_Dir;
        }

        if (type == typeof(OpenApiSecurityScheme) || type == typeof(IOpenApiSecurityScheme))
        {
            return Constants.SecurityScheme_Dir;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }


    public static async Task<string> SerializeElementAsync<T>(
        this T element, OpenApiSpecVersion version, OpenApiFormat format, CancellationToken cancellationToken = default) where T : IOpenApiSerializable
    {
        using var stream = new MemoryStream();

        if (format is OpenApiFormat.Json)
        {
            //var jsonWriter = new OpenApiJsonWriter(new StreamWriter(stream, Encoding.UTF8));
            await element.SerializeAsJsonAsync(stream, version, cancellationToken).ConfigureAwait(false);
        }
        else if (format is OpenApiFormat.Yaml)
        {
            // var yamlWriter = new OpenApiYamlWriter(new StreamWriter(stream, Encoding.UTF8));
            await element.SerializeAsYamlAsync(stream, version, cancellationToken).ConfigureAwait(false);
        }
        stream.Seek(0, SeekOrigin.Begin);

        return await new StreamReader(stream).ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> SerializeDocumentAsync(
        this OpenApiDocument document, OpenApiSpecVersion version, OpenApiFormat format, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();

        if (format is OpenApiFormat.Json)
        {
            await document.SerializeAsJsonAsync(stream, version, cancellationToken).ConfigureAwait(false);
        }
        else if (format is OpenApiFormat.Yaml)
        {
            await document.SerializeAsYamlAsync(stream, version, cancellationToken).ConfigureAwait(false);
        }

        stream.Seek(0, SeekOrigin.Begin);

        // document.Serialize(stream, version, format, new OpenApiWriterSettings
        // {
        //     InlineLocalReferences = true,
        //     InlineExternalReferences = true
        // });

        return await new StreamReader(stream).ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    public static void SaveDocumentToFile(
        this OpenApiDocument document,
        OpenApiSpecVersion version,
        OpenApiFormat format,
        string filePath,
        string? declaredVersion = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var content = document.SerializeAsync(
                version,
                format.ToStr(),
                CancellationToken.None)
            .GetAwaiter()
            .GetResult()
            .PreserveDeclaredSpecVersion(declaredVersion, format);
        File.WriteAllText(filePath, content);
    }

    public static IEnumerable<KeyValuePair<string, T>> GetComponentsWithType<T>(
        this OpenApiDocument document, string elementType) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));

        switch (elementType)
        {
            case Constants.Schema:
                return document.Components?.Schemas?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Callback:
                return document.Components?.Callbacks?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Parameter:
                return document.Components?.Parameters?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Example:
                return document.Components?.Examples?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Header:
                return document.Components?.Headers?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Link:
                return document.Components?.Links?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.Response:
                return document.Components?.Responses?.Cast<KeyValuePair<string, T>>() ?? [];
            case Constants.RequestBody:
                return document.Components?.RequestBodies?.Cast<KeyValuePair<string, T>>() ?? [];

            default:
                throw new ArgumentException($"OpenAPI type `{elementType}` not supported!");
        }
    }


    public static void DeleteAllElementsOfType(this OpenApiComponents components, string elementTypeName)
    {
        switch (elementTypeName)
        {
            case Constants.Schema:
                components.Schemas = new Dictionary<string, IOpenApiSchema>();
                break;
            case Constants.Parameter:
                components.Parameters = new Dictionary<string, IOpenApiParameter>();
                break;
            case Constants.Callback:
                components.Callbacks = new Dictionary<string, IOpenApiCallback>();
                break;
            case Constants.Example:
                components.Examples = new Dictionary<string, IOpenApiExample>();
                break;
            case Constants.Header:
                components.Headers = new Dictionary<string, IOpenApiHeader>();
                break;
            case Constants.Link:
                components.Links = new Dictionary<string, IOpenApiLink>();
                break;
            case Constants.Response:
                components.Responses = new Dictionary<string, IOpenApiResponse>();
                break;
            case Constants.RequestBody:
                components.RequestBodies = new Dictionary<string, IOpenApiRequestBody>();
                break;
        }
    }

    public static void AddExternalReferenceFor(
        this OpenApiComponents components, string elementTypeName, string key, string filePath)
    {
        ArgumentNullException.ThrowIfNull(components, nameof(components));
        ArgumentException.ThrowIfNullOrEmpty(elementTypeName, nameof(elementTypeName));
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

        switch (elementTypeName)
        {
            case Constants.Schema:
                components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
                components.Schemas[key] = new OpenApiSchemaReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Parameter:
                components.Parameters ??= new Dictionary<string, IOpenApiParameter>();
                components.Parameters[key] = new OpenApiParameterReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Callback:
                components.Callbacks ??= new Dictionary<string, IOpenApiCallback>();
                components.Callbacks[key] = new OpenApiCallbackReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Example:
                components.Examples ??= new Dictionary<string, IOpenApiExample>();
                components.Examples[key] = new OpenApiExampleReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Header:
                components.Headers ??= new Dictionary<string, IOpenApiHeader>();
                components.Headers[key] = new OpenApiHeaderReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Link:
                components.Links ??= new Dictionary<string, IOpenApiLink>();
                components.Links[key] = new OpenApiLinkReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.Response:
                components.Responses ??= new Dictionary<string, IOpenApiResponse>();
                components.Responses[key] = new OpenApiResponseReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            case Constants.RequestBody:
                components.RequestBodies ??= new Dictionary<string, IOpenApiRequestBody>();
                components.RequestBodies[key] = new OpenApiRequestBodyReference(
                    referenceId: key,
                    hostDocument: null,
                    externalResource: filePath);
                break;

            default:
                throw new NotSupportedException($"Unsupported element type '{elementTypeName}'.");
        }
    }
    
}