using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System.Diagnostics;

namespace Catdocs.Lib.OpenAPI;

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
        
        SpecLogger.SetLogFilename(inputFile);
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
            SpecLogger.Log($"File '{_inputFile}' not found!");
            throw new FileNotFoundException(nameof(_inputFile));
        }
        
        var stop_watch = new Stopwatch();
        stop_watch.Start();

        using var file_stream = new FileStream(_inputFile, FileMode.Open);
        _document = new OpenApiStreamReader().Read(file_stream, out var diagnostics);
        stop_watch.Stop();
        _parseTime = stop_watch.ElapsedMilliseconds;
        SpecLogger.Log($"Document parsed in : {_parseTime} ms");

        _hasErrors = diagnostics.Errors.Any();
        _success = !_hasErrors;

        if(_hasErrors)
        {
            foreach(var error in diagnostics.Errors)
            {
                AddError(error.ToString());
            }
            SpecLogger.Log("Document has errors!");
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
        
        _document.ExportPaths(outputDir, _version, _format);
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
        SpecLogger.Log($"Split completed in : {_splitTime} ms");
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
}
