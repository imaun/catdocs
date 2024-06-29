using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System.Diagnostics;

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


    public OpenApiSpecInfo Load()
    {
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


    public string SerializeDocument()
    {
        if(_document is null)
        {
            throw new NullReferenceException(nameof(_document));
        }

        var stream = new MemoryStream();

        _document.Serialize(
            stream,
            _version,
            _format,
            new()
            {
                InlineLocalReferences = _inlineLocal,
                InlineExternalReferences = _inlineExternal
            });

        stream.Position = 0;

        return new StreamReader(stream).ReadToEnd();
    }
}
