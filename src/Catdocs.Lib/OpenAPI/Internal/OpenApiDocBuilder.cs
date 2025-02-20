﻿using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Catdocs.Lib.OpenAPI.Internal;

internal class OpenApiDocBuilder
{
    private readonly OpenApiDocument _document;
    private OpenApiFormat _format;
    private OpenApiSpecVersion _version;
    private string _inputDir;

    public OpenApiDocBuilder(
        string inputDir, 
        OpenApiDocument document, 
        OpenApiSpecVersion version, 
        OpenApiFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDir);
        ArgumentNullException.ThrowIfNull(document);

        _document = document;
        _inputDir = inputDir;
        _version = version;
        _format = format;
    }


    public OpenApiDocument Bundle()
    {
        var api_paths = new OpenApiPaths();
        foreach (var path in _document.Paths)
        {
            if (path.Value.Reference is not null)
            {   
                var pathRef = path.Value.Reference.ExternalResource;
                var filePath = Path.Combine(_inputDir, pathRef);
                var pathDoc = LoadApiPathDocument(Path.GetFullPath(filePath));
                foreach (var resolvedPath in pathDoc.Paths)
                {
                    api_paths.Add(resolvedPath.Key, resolvedPath.Value as OpenApiPathItem);
                }
            }
            else
            {
                api_paths.Add(path.Key, path.Value);
            }
        }

        _document.Paths = api_paths;

        _document.Components ??= new OpenApiComponents();

        _document.Components.Schemas = ResolveReferences<OpenApiSchema>();
        _document.Components.Callbacks = ResolveReferences<OpenApiCallback>();
        _document.Components.Examples = ResolveReferences<OpenApiExample>();
        _document.Components.Parameters = ResolveReferences<OpenApiParameter>();
        _document.Components.Headers = ResolveReferences<OpenApiHeader>();
        _document.Components.Responses = ResolveReferences<OpenApiResponse>();
        _document.Components.RequestBodies = ResolveReferences<OpenApiRequestBody>();
        _document.Components.Links = ResolveReferences<OpenApiLink>();

        return _document;
    }


    internal OpenApiDocument LoadApiPathDocument(string filePath)
    {
        var reader = new OpenApiStreamReader(new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences,
        });
        
        using var file_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var doc = reader.Read(file_stream, out var diagnostic);

        return doc;
    }

    internal async Task<OpenApiDocument> LoadApiPathDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var reader = new OpenApiStreamReader(new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences,
        });
        
        using var file_stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var doc = await reader.ReadAsync(file_stream, cancellationToken).ConfigureAwait(false);
        return doc.OpenApiDocument;
    }

    internal Dictionary<string, T> ResolveReferences<T>() where T: IOpenApiReferenceable
    {
        var elementType = typeof(T).GetOpenApiElementTypeName();
        var result = new Dictionary<string, T>();
        var file_ext = _format.GetFormatFileExtension();
        var element_dir = Path.Combine(_inputDir, typeof(T).GetOpenApiElementDirectoryName());

        if (!Directory.Exists(element_dir))
        {
            return result;
        }
        
        var files = Directory.GetFiles(element_dir, $"*.{file_ext}");
        if (!files.Any())
        {
            return result;
        }

        foreach (var f in files)
        {
            var componentPart = LoadApiPathDocument(f);
            // if (diagnostics is not null)
            // {
            //     SpecLogger.LogError(diagnostics.GetErrorLogForElementType(elementType, f));
            // }
            
            foreach (var component in componentPart.GetComponentsWithType<T>(elementType))
            {
                result.Add(component.Key, component.Value);
            }
        }

        return result;
    }

    internal async Task<Dictionary<string, T>> ResolveReferenceAsync<T>(CancellationToken cancellationToken = default) 
        where T : IOpenApiReferenceable
    {
        var elementType = typeof(T).GetOpenApiElementTypeName();
        var result = new Dictionary<string, T>();
        var file_ext = _format.GetFormatFileExtension();
        var element_dir = Path.Combine(_inputDir, typeof(T).GetOpenApiElementDirectoryName());

        if (!Directory.Exists(element_dir))
        {
            return result;
        }
        
        var files = Directory.GetFiles(element_dir, $"*.{file_ext}");
        if (!files.Any())
        {
            return result;
        }

        foreach (var f in files)
        {
            var componentPart = await LoadApiPathDocumentAsync(f, cancellationToken);
            // if (diagnostics is not null)
            // {
            //     SpecLogger.LogError(diagnostics.GetErrorLogForElementType(elementType, f));
            // }
            
            foreach (var component in componentPart.GetComponentsWithType<T>(elementType))
            {
                result.Add(component.Key, component.Value);
            }
        }

        return result;
    }
}