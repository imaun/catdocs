using Catdocs.Lib.OpenAPI;
using Catdocs.Commands;

SpecLogger.EnableConsoleLogging();

await MainCommand.InvokeAsync(args);

Environment.Exit(0);