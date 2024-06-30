using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Writers;

namespace SpecEditor.Lib.OpenAPI;

public class OpenAPISpecParser
{
    private readonly string _inputFile;
    private long _parseTime;
    private bool _success;
    private bool _hasErrors;
    private List<string> _errors = [];
    private OpenApiSpecVersion _version;
    private OpenApiDocument _document;
    private OpenApiFormat _format;
    private bool _inlineLocal;
    private bool _inlineExternal;
    private long _splitTime;

    private void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
    }

    public OpenAPISpecParser(
        string inputFile, 
        OpenApiSpecVersion version = OpenApiSpecVersion.OpenApi3_0,
        OpenApiFormat format = OpenApiFormat.Yaml,
        bool inlineLocal = false,
        bool inlineExternal = false)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
            throw new ArgumentNullException(nameof(inputFile));

        _inputFile = inputFile;
        _version = version;
        _format = format;
        _inlineLocal = inlineLocal;
        _inlineExternal = inlineExternal;
    }

    public OpenApiDocument Document => _document ?? throw new NullReferenceException(nameof(Document));
    
    public long SplitTime { get; private set; }
    
    public OpenApiSpecInfo Load()
    {
        if (!File.Exists(_inputFile))
        {
            //TODO: Log file not found
            throw new FileNotFoundException(nameof(_inputFile));
        }
        
        var stop_watch = new Stopwatch();
        stop_watch.Start();

        using var file_stream = new FileStream(_inputFile, FileMode.Open);
        _document = new OpenApiStreamReader().Read(file_stream, out var diagnostics);
        stop_watch.Stop();
        _parseTime = stop_watch.ElapsedMilliseconds;
        //TODO: log parseTime

        _hasErrors = diagnostics.Errors.Any();
        _success = !_hasErrors;

        if(_hasErrors)
        {
            foreach(var error in diagnostics.Errors)
            {
                AddError(error.ToString());
            }
        }

        return new OpenApiSpecInfo(
            _inputFile,
            _hasErrors,
            _version.ToStr(),
            _format.ToStr(),
            _format.IsJson(),
            _format.IsYaml(),
            _errors,
            _parseTime
        );
    }

    public OpenApiStatsResult GetStats() 
    {
        var visitor = new OpenApiStatsVisitor();
        var walker = new OpenApiWalker(visitor);
        walker.Walk(_document);

        return visitor.GetStats();
    }


    public string ToJsonString() => Convert(OpenApiFormat.Json);

    public string ToYamlString() => Convert(OpenApiFormat.Yaml);
    

    private string Convert(OpenApiFormat format)
    {
        if(_document is null)
        {
            throw new NullReferenceException(nameof(_document));
        }

        var stream = new MemoryStream();

        _document.Serialize(
            stream,
            _version,
            format,
            new()
            {
                InlineLocalReferences = _inlineLocal,
                InlineExternalReferences = _inlineExternal
            });

        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
    }

    public void SplitToExternalFiles(string outputDir)
    {
        if (string.IsNullOrWhiteSpace(outputDir))
        {
            throw new ArgumentNullException(nameof(outputDir));
        }

        var stop_watch = new Stopwatch();
        stop_watch.Start();
        
        CreateDirIfNotExists(outputDir);
        
        var paths_dir = Path.Combine(outputDir, $"paths");
        CreateDirIfNotExists(paths_dir);

        foreach (var path in _document.Paths)
        {
            var path_filename = GetNormalizedPathFilename($"{paths_dir}{Path.DirectorySeparatorChar}{path.Key}.{_format.GetFormatFileExtension()}");
            using var stream = new MemoryStream();
            path.Value.Serialize(stream, _version, _format,
                new OpenApiWriterSettings
                {
                    InlineLocalReferences = _inlineLocal,
                    InlineExternalReferences = _inlineExternal
                });
            stream.Position = 0;
            var content = new StreamReader(stream).ReadToEnd();
            
            SaveToFile(path_filename, content);
        }
        
        //TODO: check if has components
        
        _document.ExportSchemas(outputDir, _version, _format);
        _document.ExportParameters(outputDir, _version, _format);
        _document.ExportExamples(outputDir, _version, _format);
        _document.ExportHeaders(outputDir, _version, _format);
        _document.ExportResponses(outputDir, _version, _format);
        _document.ExportLinks(outputDir, _version, _format);
        _document.ExportCallbacks(outputDir, _version, _format);
        _document.ExportRequestBodies(outputDir, _version, _format);
        _document.ExportSecuritySchemes(outputDir, _version, _format);
        
        stop_watch.Stop();
        _splitTime = stop_watch.ElapsedMilliseconds;
    }

    private static void SaveToFile(string filePath, string content)
    {
        var fs = new FileStream(
            filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.Write(content);
        stream_writer.Flush();
        stream_writer.Close();
    }

    private static string GetNormalizedPathFilename(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            //TODO: log error
            throw new ArgumentNullException(nameof(path));
        }

        return path.TrimStart('/').Replace('/', '_');
    }

    private static void CreateDirIfNotExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void ExportSchemas(string outputDir)
    {
        
    }
}
