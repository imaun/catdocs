using Microsoft.OpenApi;
using SpecEditor.Lib.OpenAPI;

Console.WriteLine("Hello, World!");

string fileName = "C:\\Users\\imun2\\Source\\openapi\\sample.json";

var parser = new OpenAPISpecParser(
    fileName, OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json, true, true);

var parse_result = parser.Load();

if (parse_result.HasErrors)
{
    Console.WriteLine("Has Errors: ");
    foreach (var err in parse_result.Errors)
    {
        Console.WriteLine(err);
    }
    
    Environment.Exit(-1);
}

Console.WriteLine($"Successfully parsed in {parse_result.ParseTime} ms");

var json_document = parser.Document;

Console.WriteLine($"Title: {json_document.Info.Title}");
Console.WriteLine($" {json_document.Info.Description}");

var stats = parser.GetStats();
Console.WriteLine(stats.ToString());

//var outputDir = Path.Combine(Environment.CurrentDirectory, "__out");
var outputDir = "C:\\Users\\imun2\\Source\\openapi\\__out";
Console.WriteLine($"Start splitting to {outputDir}");

parser.SplitToExternalFiles(outputDir);

Console.WriteLine($"Split finished in {parser.SplitTime} ms");

Environment.Exit(0);

var json = parser.ToJsonString();

var dest_file = "C:\\Users\\imun2\\Source\\openapi\\output.json";
await using var file_stream = new StreamWriter(dest_file);
await file_stream.WriteAsync(json);
await file_stream.FlushAsync();
file_stream.Close();

Console.WriteLine("Json file created!");

var yaml = parser.ToYamlString();

var yaml_dest_file = "C:\\Users\\imun2\\Source\\openapi\\output.yaml";
await using var yaml_file_stream = new StreamWriter(yaml_dest_file);
await yaml_file_stream.WriteAsync(yaml);
await yaml_file_stream.FlushAsync();
yaml_file_stream.Close();

Console.WriteLine("Finished!");


Environment.Exit(0);