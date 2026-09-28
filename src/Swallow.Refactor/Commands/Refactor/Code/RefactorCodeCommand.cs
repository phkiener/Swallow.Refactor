namespace Swallow.Refactor.Commands.Refactor.Code;

using System.Text.Json;
using Execution.Registration;
using Microsoft.CodeAnalysis;
using Spectre.Console.Cli;
using Abstractions.Filtering;
using Core;
using Execution;

public sealed class RefactorCodeCommand : ProgressCommand<RefactorCodeSettings>
{
    protected override async Task RunAsync(RefactorCodeSettings settings)
    {
        var configuration = await BuildConfiguration(settings);
        var documents = await RunAsync(
            name: "Fetching documents",
            func: () => LoadDocuments(solution: Workspace.CurrentSolution, documentFilters: configuration.DocumentFilters));

        using var unitOfWork = Workspace.BeginChanges();
        foreach (var document in documents)
        {
            unitOfWork.RecordChanges(documentId: document, rewriters: configuration.Rewriters);
        }

        await ProcessAsync(unitOfWork);
    }

    private async Task<RunConfiguration> BuildConfiguration(RefactorCodeSettings settings)
    {
        if (settings.RewriterSpecs is [var path] && File.Exists(path))
        {
            var config = await ReadJsonFrom<Recipe>(path);

            return RunConfiguration.For(recipe: config, documentRewriterFactory: Registry.DocumentRewriter);
        }

        return RunConfiguration.For(
            fileNameRegex: settings.FileName,
            content: settings.Content,
            rewriterExpressions: settings.RewriterSpecs,
            documentRewriterFactory: Registry.DocumentRewriter);
    }

    private static async Task<T> ReadJsonFrom<T>(string path)
    {
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(utf8Json: stream, options: serializerOptions)
               ?? throw new InvalidOperationException($"Failed to parse rewriter configuration from {path}.");
    }

    private static async IAsyncEnumerable<DocumentId> LoadDocuments(Solution solution, IEnumerable<IDocumentFilter> documentFilters)
    {
        var enumeratedFilters = documentFilters.ToList();
        foreach (var document in solution.Projects.SelectMany(p => p.Documents))
        {
            foreach (var filter in enumeratedFilters)
            {
                if (await filter.Matches(document))
                {
                    yield return document.Id;

                    break;
                }
            }
        }
    }

    public static ICommandConfigurator Register(IConfigurator<RefactorCommandSettings> configurator)
    {
        return configurator.Register<RefactorCodeCommand, RefactorCodeSettings>(
            name: "code",
            description: "Run rewriters over all files (or a subset thereof) in a solution",
            [
                ["refactor", "-s", "path/to/solution.sln", "code", "OptimizeUsings"],
                ["refactor", "code", "--filter-content", "logger", "config.json"]
            ]);
    }
}
