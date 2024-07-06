using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Catdocs.Lib.OpenAPI;

public static class ExportExtensions
{
    
    public static string GetOpenApiElementTypeName<T>(T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => OpenApiConstants.Schema,
            OpenApiParameter => OpenApiConstants.Parameter,
            OpenApiExample => OpenApiConstants.Example,
            OpenApiHeader => OpenApiConstants.Header,
            OpenApiResponse => OpenApiConstants.Response,
            OpenApiRequestBody => OpenApiConstants.RequestBody,
            OpenApiLink => OpenApiConstants.Link,
            OpenApiCallback => OpenApiConstants.Callback,
            OpenApiSecurityScheme => OpenApiConstants.SecurityScheme,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static string GetOpenApiElementTypeName(this Type type)
    {
        if (type == typeof(OpenApiSchema))
        {
            return OpenApiConstants.Schema;
        }

        if (type == typeof(OpenApiParameter))
        {
            return OpenApiConstants.Parameter;
        }

        if (type == typeof(OpenApiExample))
        {
            return OpenApiConstants.Example;
        }

        if (type == typeof(OpenApiHeader))
        {
            return OpenApiConstants.Header;
        }

        if (type == typeof(OpenApiResponse))
        {
            return OpenApiConstants.Response;
        }

        if (type == typeof(OpenApiRequestBody))
        {
            return OpenApiConstants.RequestBody;
        }

        if (type == typeof(OpenApiLink))
        {
            return OpenApiConstants.Link;
        }

        if (type == typeof(OpenApiCallback))
        {
            return OpenApiConstants.Callback;
        }

        if (type == typeof(OpenApiSecurityScheme))
        {
            return OpenApiConstants.SecurityScheme;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }

    private static string GetOpenApiElementDirectoryName<T>(this T element) where T : IOpenApiReferenceable
    {
        ArgumentNullException.ThrowIfNull(element);

        return element switch
        {
            OpenApiSchema => OpenApiConstants.Schema_Dir,
            OpenApiParameter => OpenApiConstants.Parameter_Dir,
            OpenApiExample => OpenApiConstants.Example_Dir,
            OpenApiHeader => OpenApiConstants.Header_Dir,
            OpenApiResponse => OpenApiConstants.Response_Dir,
            OpenApiRequestBody => OpenApiConstants.RequestBody_Dir,
            OpenApiLink => OpenApiConstants.Link_Dir,
            OpenApiCallback => OpenApiConstants.Callback_Dir,
            _ => throw new NotSupportedException("OpenAPI type not supported!")
        };
    }

    public static string GetOpenApiElementDirectoryName(this Type type)
    {
        if (type == typeof(OpenApiSchema))
        {
            return OpenApiConstants.Schema_Dir;
        }

        if (type == typeof(OpenApiParameter))
        {
            return OpenApiConstants.Parameter_Dir;
        }

        if (type == typeof(OpenApiExample))
        {
            return OpenApiConstants.Example_Dir;
        }

        if (type == typeof(OpenApiHeader))
        {
            return OpenApiConstants.Header_Dir;
        }

        if (type == typeof(OpenApiResponse))
        {
            return OpenApiConstants.Response_Dir;
        }

        if (type == typeof(OpenApiRequestBody))
        {
            return OpenApiConstants.RequestBody_Dir;
        }

        if (type == typeof(OpenApiLink))
        {
            return OpenApiConstants.Link_Dir;
        }

        if (type == typeof(OpenApiCallback))
        {
            return OpenApiConstants.Callback_Dir;
        }

        throw new NotSupportedException("OpenAPI type not supported!");
    }


    public static string SerializeElement<T>(
        this T element, OpenApiSpecVersion version, OpenApiFormat format) where T: IOpenApiReferenceable
    {
        using var stream = new MemoryStream();
        element.Serialize(stream, version, format, new OpenApiWriterSettings
        {
            InlineLocalReferences = false,
            InlineExternalReferences = true
        });
        stream.Position = 0;
        
        return new StreamReader(stream).ReadToEnd();
    }


    public static string SerializeDocument(
        this OpenApiDocument document, OpenApiSpecVersion version, OpenApiFormat format)
    {
        using var stream = new MemoryStream();
        document.Serialize(stream, version, format, new OpenApiWriterSettings
        {
            InlineLocalReferences = true,
            InlineExternalReferences = true
        });
        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
    }

    public static void SaveDocumentToFile(this OpenApiDocument document, OpenApiFormat format, string filename)
    {
        using var stream = new FileStream(filename, FileMode.Create);
        if (format is OpenApiFormat.Yaml)
        {
            var yamlWriter = new OpenApiYamlWriter(new StreamWriter(stream));
            document.SerializeAsV3(yamlWriter);
        }
        else
        {
            var jsonWriter = new OpenApiJsonWriter(new StreamWriter(stream));
            document.SerializeAsV3(jsonWriter);
        }
    }

    
    public static void DeleteAllElementsOfType(this OpenApiComponents components, string elementTypeName)
    {
        switch (elementTypeName)
        {
            case OpenApiConstants.Schema:
                components.Schemas = new Dictionary<string, OpenApiSchema>();
                break;
            case OpenApiConstants.Parameter:
                components.Parameters = new Dictionary<string, OpenApiParameter>();
                break;
            case OpenApiConstants.Callback:
                components.Callbacks = new Dictionary<string, OpenApiCallback>();
                break;
            case OpenApiConstants.Example:
                components.Examples = new Dictionary<string, OpenApiExample>();
                break;
            case OpenApiConstants.Header:
                components.Headers = new Dictionary<string, OpenApiHeader>();
                break;
            case OpenApiConstants.Link:
                components.Links = new Dictionary<string, OpenApiLink>();
                break;
            case OpenApiConstants.Response:
                components.Responses = new Dictionary<string, OpenApiResponse>();
                break;
            case OpenApiConstants.RequestBody:
                components.RequestBodies = new Dictionary<string, OpenApiRequestBody>();
                break;
        }
    }

    public static void AddExternalReferenceFor(
        this OpenApiComponents components, string elementTypeName, string key, string filePath)
    {
        switch (elementTypeName)
        {
            case OpenApiConstants.Schema:
                components.Schemas.Add(key, new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Parameter:
                components.Parameters.Add(key, new OpenApiParameter
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Callback:
                components.Callbacks.Add(key, new OpenApiCallback
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Example:
                components.Examples.Add(key, new OpenApiExample
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Header:
                components.Headers.Add(key, new OpenApiHeader
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Link:
                components.Links.Add(key, new OpenApiLink
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.Response:
                components.Responses.Add(key, new OpenApiResponse
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
            
            case OpenApiConstants.RequestBody:
                components.RequestBodies.Add(key, new OpenApiRequestBody
                {
                    Reference = new OpenApiReference
                    {
                        Id = key,
                        Type = elementTypeName.GetOpenApiReferenceType(),
                        ExternalResource = filePath
                    }
                });
                break;
        }
    }
}