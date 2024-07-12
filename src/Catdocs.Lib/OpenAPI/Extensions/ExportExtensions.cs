using System.Text;
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
        this T element, OpenApiSpecVersion version, OpenApiFormat format) where T: IOpenApiSerializable
    {
        using var stream = new MemoryStream();
        if (format is OpenApiFormat.Json)
        {
            var jsonWriter = new OpenApiJsonWriter(new StreamWriter(stream, Encoding.UTF8));
            element.Serialize(jsonWriter, version);
        }
        else
        {
            var yamlWriter = new OpenApiYamlWriter(new StreamWriter(stream, Encoding.UTF8));
            element.Serialize(yamlWriter, version);
        }
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

    public static void SaveDocumentToFile(
        this OpenApiDocument document, OpenApiSpecVersion version, OpenApiFormat format, string filename)
    {
        using var stream = new FileStream(filename, FileMode.Create);
        if (format is OpenApiFormat.Yaml)
        {
            var yamlWriter = new OpenApiYamlWriter(new StreamWriter(stream));
            if (version is OpenApiSpecVersion.OpenApi3_0)
            {
                document.SerializeAsV3(yamlWriter);    
            }
            else
            {
                document.SerializeAsV2(yamlWriter);
            }
            yamlWriter.Flush();
        }
        else if(format is OpenApiFormat.Json)
        {
            var jsonWriter = new OpenApiJsonWriter(new StreamWriter(stream));
            if (version is OpenApiSpecVersion.OpenApi3_0)
            {
                document.SerializeAsV3(jsonWriter);    
            }
            else
            {
                document.SerializeAsV2(jsonWriter);
            }
            jsonWriter.Flush();
        }
        //NOT SUPPORTED!
        else
        {
            SpecLogger.Log("NOT SUPPORTED DOCUMENT FORMAT!");
        }
    }

    public static IEnumerable<KeyValuePair<string, T>> GetComponentsWithType<T>(
        this OpenApiDocument document, string elementType) where T : IOpenApiReferenceable
    {
        switch (elementType)
        {
            case OpenApiConstants.Schema:
                return document.Components.Schemas.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Callback:
                return document.Components.Callbacks.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Parameter:
                return document.Components.Parameters.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Example:
                return document.Components.Examples.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Header:
                return document.Components.Headers.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Link:
                return document.Components.Links.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.Response:
                return document.Components.Responses.Cast<KeyValuePair<string, T>>();
            case OpenApiConstants.RequestBody:
                return document.Components.RequestBodies.Cast<KeyValuePair<string, T>>();
            
            default:
                throw new ArgumentException($"OpenAPI type `{elementType}` not supported!");
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