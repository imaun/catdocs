using System.IO;

namespace Catdocs.Editor.OpenApi;

public class OpenApiDocumentFile
{

    public OpenApiDocumentFile(FileInfo fileInfo)
    {
        FilePath = fileInfo.FullName;
        FileName = fileInfo.Name;
        Extension = fileInfo.Extension;
    }
    
    public string FilePath { get; }
    
    public string FileName { get; }
    
    public string Extension { get; }
}