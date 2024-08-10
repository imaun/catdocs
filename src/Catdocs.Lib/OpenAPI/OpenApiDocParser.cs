using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System.Diagnostics;
using Catdocs.Lib.OpenAPI.Internal;

namespace Catdocs.Lib.OpenAPI;

public class OpenApiDocParser
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
    private long _buildTime;
    private long _convertTime;

    private void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
    }

    public OpenApiDocParser(
        string inputPath, 
        OpenApiSpecVersion version = OpenApiSpecVersion.OpenApi3_0,
        OpenApiFormat format = OpenApiFormat.Yaml,
        bool inlineLocal = false,
        bool inlineExternal = false)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
            throw new ArgumentNullException(nameof(inputPath));
        
        SpecLogger.SetLogFilename(inputPath);
        _inputFile = GetDocumentFilenameFromPath(inputPath);
        _version = version;
        _format = format;
        _inlineLocal = inlineLocal;
        _inlineExternal = inlineExternal;
    }

    public OpenApiDocument Document => _document ?? throw new NullReferenceException(nameof(Document));

    public long SplitTime => _splitTime;

    public long BuildTime => _buildTime;

    public long ConvertTime => _convertTime;

    public long ParseTime => _parseTime;
    
    public OpenApiSpecInfo Load()
    {
        if (!File.Exists(_inputFile))
        {
            SpecLogger.Log($"File '{_inputFile}' not found!");
            throw new FileNotFoundException(nameof(_inputFile));
        }
        
        var stop_watch = new Stopwatch();
        stop_watch.Start();

        var reader = new OpenApiStreamReader();
        using var file_stream = new FileStream(_inputFile, FileMode.Open);
        _document = reader.Read(file_stream, out var diagnostics);
        
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
            _document,
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


    public void ConvertTo(OpenApiFormat format, string targetFilename)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(targetFilename);

        var stop_watch = new Stopwatch();
        stop_watch.Start();
        
        string output = string.Empty;

        if (format is OpenApiFormat.Json)
        {
            output = ToJsonString();
        }
        else if (format is OpenApiFormat.Yaml)
        {
            output = ToYamlString();
        }

        SaveToFile(filePath: targetFilename, content: output);
        
        stop_watch.Stop();
        _convertTime = stop_watch.ElapsedMilliseconds;
    }

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

    public void Split(string outputDir)
    {
        if (string.IsNullOrWhiteSpace(outputDir))
        {
            throw new ArgumentNullException(nameof(outputDir));
        }

        var stop_watch = new Stopwatch();
        stop_watch.Start();

        var splitter = new OpenApiDocSplitter(outputDir, _document, _version, _format);
        splitter.Split();
        
        //TODO: check if has components
        stop_watch.Stop();
        _splitTime = stop_watch.ElapsedMilliseconds;
        SpecLogger.Log($"Split completed in : {_splitTime} ms");
    }


    public void Build(string newDocumentFilename)
    {
        if (string.IsNullOrWhiteSpace(newDocumentFilename))
        {
            throw new ArgumentNullException(nameof(newDocumentFilename));
        }

        var stop_watch = new Stopwatch();
        stop_watch.Start();

        var inputDir = GetDirectoryForFilename(_inputFile);

        var builder = new OpenApiDocBuilder(inputDir, _document, _version, _format);
        var new_document = builder.Build();
        
        new_document.SaveDocumentToFile(_version, _format, newDocumentFilename);
        
        stop_watch.Stop();
        _buildTime = stop_watch.ElapsedMilliseconds;
        SpecLogger.Log($"Build completed in : {_buildTime} ms");
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

    internal string GetDirectoryForFilename(string filename)
    {
        var fileInfo = new FileInfo(filename);
        return fileInfo.DirectoryName;
    }
    
    internal string GetDocumentFilenameFromPath(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return inputPath;
        }

        if (Directory.Exists(inputPath))
        {
            var file = Directory
                .EnumerateFiles(inputPath, "*.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                     f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));

            if (file is not null)
            {
                return file;
            }
        }
        
        string err = $"No OpenAPI files found in: {inputPath}";
        SpecLogger.LogError(err);
        throw new FileNotFoundException(err); 
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
}
