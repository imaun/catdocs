using Catdocs.Lib.OpenAPI;
using Microsoft.OpenApi;

namespace Catdocs.Tests.OpenApiTests;

public class OpenApiEndToEndTests
{
    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, OpenApiFormat.Json)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml)]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)]
    public void Parse_valid_documents_from_temporary_directories(
        OpenApiSpecVersion version,
        OpenApiFormat format)
    {
        using var temp = new TemporaryDirectory();
        var input = temp.WriteFile(
            $"valid.{GetExtension(format)}",
            GetValidDocument(version, format));

        var result = new OpenApiDocParser(input, version, format).Load();

        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Single(result.Document.Paths);
    }

    [Theory]
    [InlineData(OpenApiFormat.Yaml)]
    [InlineData(OpenApiFormat.Json)]
    public void Parse_invalid_documents_reports_diagnostics(OpenApiFormat format)
    {
        using var temp = new TemporaryDirectory();
        var input = temp.WriteFile(
            $"invalid.{GetExtension(format)}",
            format == OpenApiFormat.Json
                ? """{"openapi":"3.0.1","info":{"title":"Invalid","version":"1.0.0"},"paths":{"/pets":{"get":{"responses":{"200":{}}}}}}"""
                : """
                  openapi: 3.0.1
                  info:
                    title: Invalid
                    version: 1.0.0
                  paths:
                    /pets:
                      get:
                        responses:
                          '200': { }
                  """);

        var result = new OpenApiDocParser(
            input,
            OpenApiSpecVersion.OpenApi3_0,
            format).Load();

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Convert_json_to_yaml_and_back_preserves_the_document()
    {
        using var temp = new TemporaryDirectory();
        var json = temp.WriteFile(
            "source.json",
            GetValidDocument(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json));
        var yaml = temp.GetPath("converted.yaml");
        var roundTripJson = temp.GetPath("round-trip.json");

        var jsonParser = new OpenApiDocParser(
            json,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Json);
        Assert.True(jsonParser.Load().Success);
        jsonParser.ConvertTo(OpenApiFormat.Yaml, yaml);

        var yamlParser = new OpenApiDocParser(
            yaml,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml);
        Assert.True(yamlParser.Load().Success);
        yamlParser.ConvertTo(OpenApiFormat.Json, roundTripJson);

        var roundTrip = new OpenApiDocParser(
            roundTripJson,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Json).Load();

        Assert.True(roundTrip.Success);
        Assert.Contains("/pets", roundTrip.Document.Paths.Keys);
        var roundTripComponents = roundTrip.Document.Components
            ?? throw new InvalidDataException("Converted document has no components.");
        var roundTripSchemas = roundTripComponents.Schemas
            ?? throw new InvalidDataException("Converted document has no schemas.");
        Assert.Contains("Pet", roundTripSchemas.Keys);
    }

    [Fact]
    public void Split_bundle_and_validate_preserves_paths_components_and_references()
    {
        using var temp = new TemporaryDirectory();
        var source = temp.WriteFile(
            "source.yaml",
            GetValidDocument(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml));
        var splitDirectory = temp.GetPath("split");
        var bundledFile = temp.GetPath("bundled.yaml");

        var sourceParser = new OpenApiDocParser(
            source,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml);
        Assert.True(sourceParser.Load().Success);
        sourceParser.Split(splitDirectory);

        var splitMain = Path.Combine(splitDirectory, "OpenApi.yaml");
        var splitText = File.ReadAllText(splitMain);
        Assert.Contains("paths/pets.yaml#/paths/~1pets", splitText);
        Assert.Contains("schemas/Pet.yaml#/components/schemas/Pet", splitText);
        Assert.True(File.Exists(Path.Combine(splitDirectory, "paths", "pets.yaml")));
        Assert.True(File.Exists(Path.Combine(splitDirectory, "schemas", "Pet.yaml")));

        var splitParser = new OpenApiDocParser(
            splitMain,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml);
        Assert.True(splitParser.Load().Success);
        splitParser.Bundle(bundledFile);

        var bundled = new OpenApiDocParser(
            bundledFile,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml).Load();
        var bundledText = File.ReadAllText(bundledFile);

        Assert.True(bundled.Success);
        Assert.Contains("/pets", bundled.Document.Paths.Keys);
        var bundledComponents = bundled.Document.Components
            ?? throw new InvalidDataException("Bundled document has no components.");
        var bundledSchemas = bundledComponents.Schemas
            ?? throw new InvalidDataException("Bundled document has no schemas.");
        Assert.Contains("Pet", bundledSchemas.Keys);
        Assert.Contains("#/components/schemas/Pet", bundledText);
        Assert.DoesNotContain("schemas/Pet.yaml", bundledText);
        Assert.DoesNotContain("paths/pets.yaml", bundledText);
    }

    [Fact]
    public void Repository_example_pipeline_bundles_and_validates()
    {
        using var temp = new TemporaryDirectory();
        var exampleDirectory = FindRepositoryPath("examples", "bundle-pipeline");
        CopyDirectory(exampleDirectory, temp.Path);
        var input = temp.GetPath("OpenApi.yaml");
        var output = temp.GetPath("pipeline-output.yaml");

        var parser = new OpenApiDocParser(
            input,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml);
        Assert.True(parser.Load().Success);
        parser.Bundle(output);

        var result = new OpenApiDocParser(
            output,
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Yaml).Load();

        Assert.True(result.Success);
        Assert.Equal(3, result.Document.Paths.Count);
        var components = result.Document.Components
            ?? throw new InvalidDataException("Pipeline output has no components.");
        var schemas = components.Schemas
            ?? throw new InvalidDataException("Pipeline output has no schemas.");
        Assert.Contains("Pet", schemas.Keys);
    }

    private static string GetValidDocument(OpenApiSpecVersion version, OpenApiFormat format)
    {
        if (version == OpenApiSpecVersion.OpenApi2_0)
        {
            return format == OpenApiFormat.Json
                ? """
                  {"swagger":"2.0","info":{"title":"Pets","version":"1.0.0"},"paths":{"/pets":{"get":{"responses":{"200":{"description":"OK","schema":{"$ref":"#/definitions/Pet"}}}}}},"definitions":{"Pet":{"type":"object","properties":{"name":{"type":"string"}}}}}
                  """
                : """
                  swagger: '2.0'
                  info:
                    title: Pets
                    version: 1.0.0
                  paths:
                    /pets:
                      get:
                        responses:
                          '200':
                            description: OK
                            schema:
                              $ref: '#/definitions/Pet'
                  definitions:
                    Pet:
                      type: object
                      properties:
                        name:
                          type: string
                  """;
        }

        return format == OpenApiFormat.Json
            ? """
              {"openapi":"3.0.1","info":{"title":"Pets","version":"1.0.0"},"paths":{"/pets":{"get":{"responses":{"200":{"description":"OK","content":{"application/json":{"schema":{"$ref":"#/components/schemas/Pet"}}}}}}}},"components":{"schemas":{"Pet":{"type":"object","properties":{"name":{"type":"string"}}}}}}
              """
            : """
              openapi: 3.0.1
              info:
                title: Pets
                version: 1.0.0
              paths:
                /pets:
                  get:
                    responses:
                      '200':
                        description: OK
                        content:
                          application/json:
                            schema:
                              $ref: '#/components/schemas/Pet'
              components:
                schemas:
                  Pet:
                    type: object
                    properties:
                      name:
                        type: string
              """;
    }

    private static string GetExtension(OpenApiFormat format)
        => format == OpenApiFormat.Json ? "json" : "yaml";

    private static string FindRepositoryPath(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. segments]);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Unable to find repository path '{Path.Combine(segments)}'.");
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(source))
        {
            CopyDirectory(
                directory,
                Path.Combine(destination, Path.GetFileName(directory)));
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"catdocs-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string GetPath(params string[] segments)
            => System.IO.Path.Combine([Path, .. segments]);

        public string WriteFile(string name, string content)
        {
            var filePath = GetPath(name);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
