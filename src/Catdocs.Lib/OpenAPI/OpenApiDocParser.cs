using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using System.Diagnostics;
using Catdocs.Lib.OpenAPI.Internal;
using Catdocs.Lib.OpenAPI.Extensions;

namespace Catdocs.Lib.OpenAPI;

public class OpenApiDocParser
{
    private readonly string _inputFile;
    private long _parseTime;
    private bool _success;
    private bool _hasErrors;
    private List<string> _errors = [];
    private OpenApiSpecVersion _version;
    private OpenApiDocument _document = null!;
    private OpenApiFormat _format;
    private bool _inlineLocal;
    private bool _inlineExternal;
    private string? _declaredVersion;
    private long _splitTime;
    private long _bundleTime;
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

    public long BundleTime => _bundleTime;

    public long ConvertTime => _convertTime;

    public long ParseTime => _parseTime;
    
    public async Task<OpenApiSpecInfo> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_inputFile))
        {
            SpecLogger.Log($"File '{_inputFile}' not found!");
            throw new FileNotFoundException(nameof(_inputFile));
        }
        
        var stop_watch = new Stopwatch();
        stop_watch.Start();

        using var file_stream = new FileStream(
            _inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        _declaredVersion = OpenApiExtensions.GetDeclaredSpecVersion(
            await File.ReadAllTextAsync(_inputFile, cancellationToken).ConfigureAwait(false),
            _format);
        var readResult = await OpenApiDocument.LoadAsync(
                file_stream,
                _format.ToStr(),
                OpenApiExtensions.CreateReaderSettings(_inputFile),
                cancellationToken)
            .ConfigureAwait(false);
        _document = readResult.Document
            ?? throw new InvalidDataException($"Unable to parse OpenAPI document '{_inputFile}'.");
        var diagnostics = readResult.Diagnostic
            ?? throw new InvalidDataException($"No diagnostics returned for '{_inputFile}'.");
        
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


    public Task<string> ToJsonStringAsync(CancellationToken cancellationToken = default)
        => ConvertAsync(OpenApiFormat.Json, cancellationToken);

    public Task<string> ToYamlStringAsync(CancellationToken cancellationToken = default)
        => ConvertAsync(OpenApiFormat.Yaml, cancellationToken);


    public async Task ConvertToAsync(OpenApiFormat format, string targetFilename, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(targetFilename);

        var stop_watch = new Stopwatch();
        stop_watch.Start();
        
        var output = await ConvertAsync(format, cancellationToken).ConfigureAwait(false);

        await SaveToFileAsync(filePath: targetFilename, content: output, cancellationToken).ConfigureAwait(false);
        
        stop_watch.Stop();
        _convertTime = stop_watch.ElapsedMilliseconds;
    }

    private async Task<string> ConvertAsync(OpenApiFormat format, CancellationToken cancellationToken = default)
    {
        if(_document is null)
        {
            throw new NullReferenceException(nameof(_document));
        }

        using var stream = new MemoryStream();
        await _document.SerializeAsync(
                stream,
                _version,
                format.ToStr(),
                new OpenApiWriterSettings
                {
                    InlineLocalReferences = _inlineLocal,
                    InlineExternalReferences = _inlineExternal
                },
                cancellationToken)
            .ConfigureAwait(false);

        stream.Position = 0;

        var content = await new StreamReader(stream).ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return content.PreserveDeclaredSpecVersion(_declaredVersion, format);
    }

    public async Task SplitAsync(string outputDir, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(outputDir))
        {
            throw new ArgumentNullException(nameof(outputDir));
        }

        var stop_watch = new Stopwatch();
        stop_watch.Start();

        var splitter = new OpenApiDocSplitter(
            outputDir,
            _document,
            _version,
            _format,
            _declaredVersion);
        await splitter.SplitAsync(cancellationToken).ConfigureAwait(false);
        
        //TODO: check if has components
        stop_watch.Stop();
        _splitTime = stop_watch.ElapsedMilliseconds;
        SpecLogger.Log($"Split completed in : {_splitTime} ms");
    }


    public async Task BundleAsync(string newDocumentFilename, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newDocumentFilename))
        {
            throw new ArgumentNullException(nameof(newDocumentFilename));
        }

        var stop_watch = new Stopwatch();
        stop_watch.Start();

        var inputDir = GetDirectoryForFilename(_inputFile);

        var builder = new OpenApiDocBuilder(inputDir, _document, _version, _format);
        var new_document = await builder.BundleAsync(cancellationToken).ConfigureAwait(false);
        
        await new_document.SaveDocumentToFileAsync(
            _version,
            _format,
            newDocumentFilename,
            _declaredVersion,
            cancellationToken).ConfigureAwait(false);
        
        stop_watch.Stop();
        _bundleTime = stop_watch.ElapsedMilliseconds;
        SpecLogger.Log($"Build completed in : {_bundleTime} ms");
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
        return fileInfo.DirectoryName
            ?? throw new InvalidOperationException($"Unable to determine directory for '{filename}'.");
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
    
    private static async Task SaveToFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        await using var fs = new FileStream(
            filePath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true);
        await using var stream_writer = new StreamWriter(fs);
        await stream_writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
        await stream_writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
