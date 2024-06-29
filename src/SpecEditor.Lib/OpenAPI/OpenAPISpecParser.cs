using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecEditor.Lib.OpenAPI;

public class OpenAPISpecParser
{
    private readonly string _inputFile;
    private long _parseTime;
    private bool _success;
    private bool _hasError;
    private List<string> _errors = [];
    private OpenApiSpecVersion _version;
    private OpenApiDocument _document;

    private void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
    }

    public OpenAPISpecParser(string inputFile, OpenApiSpecVersion version = OpenApiSpecVersion.OpenApi3_0)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
            throw new ArgumentNullException(nameof(inputFile));

        _inputFile = inputFile;
        _version = version;
    }


    public OpenApiDocument Load()
    {
        var stop_watch = new Stopwatch();
        stop_watch.Start();

        using var file_stream = new FileStream(_inputFile, FileMode.Open);
        _document = new OpenApiStreamReader().Read(file_stream, out var diagnostics);
        stop_watch.Stop();
        _parseTime = stop_watch.ElapsedMilliseconds;
        //TODO: log parseTime

        _hasError = diagnostics.Errors.Any();
        _success = !_hasError;

        if(_hasError)
        {
            foreach(var error in diagnostics.Errors)
            {
                AddError(error.ToString());
            }
        }

        return _document;
    }
}
