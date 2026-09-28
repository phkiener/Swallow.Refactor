namespace Swallow.Refactor.Commands.Unused;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Execution;
using Execution.Registration;
using static Core.Prelude;

public sealed class FindUnusedCommand : ProgressCommand<FindUnusedSettings>, IRegisterableCommand
{
    protected override async Task RunAsync(FindUnusedSettings settings)
    {
        var project = Workspace.CurrentSolution.OpenProject(settings.Project);
        var compilation = await RunAsync(name: $"Compiling {settings.Project}", func: async () => await CompileProject(project: project));
        if (compilation is null)
        {
            Logger.LogWarning(message: "Project {name} does not support compilation", project.Name);

            return;
        }

        await ProcessAsync(
            name: t => $"File: {Path.GetFileName(t.FilePath)}",
            source: GetSyntaxTrees(compilation),
            func: async t => await ProcessAsync(
                name: s => $"Symbol: {s.ToDisplayString(ShortSymbolDisplayFormat)}",
                source: GetSymbols(syntaxTree: t, compilation: compilation),
                func: async symbol => await AnalyzeReferences(symbol: symbol, project: project)));
    }

    private static async Task<Compilation?> CompileProject(Project project)
    {
        return await project.GetCompilationAsync();
    }

    private static IEnumerable<SyntaxTree> GetSyntaxTrees(Compilation compilation)
    {
        return compilation.SyntaxTrees.Where(st => !st.FilePath.EndsWith(value: ".g.cs", comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<ISymbol> GetSymbols(SyntaxTree syntaxTree, Compilation compilation)
    {
        var relevantSyntaxNodes
            = syntaxTree.GetRoot().DescendantNodes().Where(s => s is MethodDeclarationSyntax or PropertyDeclarationSyntax).ToList();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var symbolFilters = Registry.SymbolFilter.List().Select(s => Registry.SymbolFilter.Create(s.Name)).ToList();

        return relevantSyntaxNodes.Select(n => semanticModel.GetDeclaredSymbol(declaration: n))
            .Where(s => s is not null && symbolFilters.All(f => f.Ignore(symbol: s) is false))
            .Cast<ISymbol>();
    }

    private async Task AnalyzeReferences(ISymbol symbol, Project project)
    {
        Logger.LogTrace(message: "{symbol}", symbol.ToDisplayString(ShortSymbolDisplayFormat));
        var location = symbol.Locations.First().GetLineSpan();
        var references = await SymbolFinder.FindReferencesAsync(symbol: symbol, solution: project.Solution);
        var referencingProjects = references.SelectMany(r => r.Locations).Select(l => l.Document.Project).ToHashSet();
        if (referencingProjects.Count == 0)
        {
            Logger.LogInformation(
                message: "[{path}] {message} (line {span})",
                location.Path,
                $"'{symbol.Name}' is never referenced",
                location.StartLinePosition.Line);
        }
        else if (referencingProjects.All(p => p != project && p.Name.Contains(value: "Test", comparisonType: StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogInformation(
                message: "[{path}] {message} (line {span})",
                location.Path,
                $"'{symbol.Name}' is only called in test projects",
                location.StartLinePosition.Line);
        }
        else if (referencingProjects.Count == 1 && referencingProjects.Single() != project)
        {
            Logger.LogInformation(
                message: "[{path}] {message} (line {span})",
                location.Path,
                $"'{symbol.Name}' is only called in {referencingProjects.Single()}",
                location.StartLinePosition.Line);
        }
    }

    public static ICommandConfigurator RegisterWith(IConfigurator configurator)
    {
        return configurator.Register<FindUnusedCommand>(
            name: "unused",
            description: "Find unused symbols in a project",
            new[]
            {
                "unused",
                "-o",
                "path/to/log.txt",
                "-s",
                "path/to/solution.sln",
                "My.Other.Project"
            });
    }
}
