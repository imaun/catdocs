using System.Collections.Generic;
using System.IO;
using Catdocs.Lib.OpenAPI;
using Microsoft.OpenApi.Models;

namespace Catdocs.Editor.OpenApi;

public class OpenApiDocumentFile
{
    
    public OpenApiDocumentFile(FileInfo fileInfo, bool loadDocument = false)
    {
        FilePath = fileInfo.FullName;
        FileName = fileInfo.Name;
        Extension = fileInfo.Extension;
        Errors = new List<string>();
        
        if (loadDocument)
        {
            Info = Load();
            Document = Info.Document;
        }
    }
    
    public string FilePath { get; }
    
    public string FileName { get; }
    
    public string Extension { get; }
    
    public OpenApiDocument? Document { get; }
    
    public OpenApiSpecInfo? Info { get; }
    
    public IReadOnlyCollection<string> Errors { get; }

    private OpenApiSpecInfo Load()
    {
        var parser = new OpenApiDocParser(FilePath);
        return parser.Load();
    }
}