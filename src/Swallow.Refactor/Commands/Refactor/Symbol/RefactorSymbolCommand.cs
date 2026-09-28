namespace Swallow.Refactor.Commands.Refactor.Symbol;

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using Execution;
using Execution.Registration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Spectre.Console.Cli;

public sealed partial class RefactorSymbolCommand : ProgressCommand<RefactorSymbolSettings>
{
    protected override async Task RunAsync(RefactorSymbolSettings settings)
    {
        if (settings.Cursor is null && settings.Type is null)
        {
            Logger.LogError("You have to pass either a cursor or a type plus optionally a member.");

            return;
        }

        var symbol = settings.Cursor is not null
            ? await RunAsync("Finding symbol", () => FindSymbolUnderCursor(settings.Cursor))
            : await RunAsync("Finding symbol", () => FindSymbol(settings.Type!, settings.Member));

        Logger.LogTrace("Found symbol {Name}", symbol!.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

        var rewriters = settings.RewriterSpecs.Select(ParseExpression);
        var solutionEditor = new SolutionEditor(Workspace.CurrentSolution);

        await ProcessAsync(spec => spec.Name, rewriters,
            async spec =>
            {
                var rewriter = Registry.TargetedRewriter.Create(spec.Name, spec.Parameters);
                await rewriter.RunAsync(solutionEditor, symbol);
            });

        Workspace.TryApplyChanges(solutionEditor.GetChangedSolution());
    }

    private static (string Name, string[] Parameters) ParseExpression(string expression)
    {
        var parsedExpression = SyntaxFactory.ParseExpression(expression);

        return parsedExpression switch
        {
            IdentifierNameSyntax identifier => (identifier.Identifier.Text, Array.Empty<string>()),
            InvocationExpressionSyntax invocation => (invocation.Expression.ToString(),
                invocation.ArgumentList.Arguments.Select(a => a.Expression.ToString()).ToArray()),
            _ => throw new InvalidOperationException(
                $"Failed to parse rewriter-expression {expression}; only names (like \"MyRewriter\") or invocations (like \"MyRewriter(\"param\", \"param\")\") are allowed.")
        };
    }

    private async Task<ISymbol?> FindSymbolUnderCursor(string cursor)
    {
        var (file, line, column) = ParseCursor(cursor);
        var matchingDocuments = Workspace.CurrentSolution.GetDocumentIdsWithFilePath(file);
        if (matchingDocuments is not [var documentId])
        {
            throw new ArgumentException($"Found multiple documents for file path '{file}'");
        }

        var document = Workspace.CurrentSolution.GetDocument(documentId)!;
        var rootNode = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return rootNode!.DescendantNodes()
            .Where(n => Contains(n.GetLocation().GetLineSpan(), line, column))
            .Select(n => semanticModel!.GetDeclaredSymbol(n) ?? semanticModel!.GetSymbolInfo(n).Symbol)
            .LastOrDefault(s => s is not null) ?? throw new ArgumentException($"Could not find a symbol under cursor '{cursor}'");
    }

    private static (string File, int Line, int Column) ParseCursor(string cursor)
    {
        var match = CursorRegex().Match(cursor);
        if (match.Success is false)
        {
            throw new ArgumentException($"'{cursor}' is not a valid cursor.");
        }

        return (match.Groups[1].Value, int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
    }

    private static bool Contains(FileLinePositionSpan span, int line, int column)
    {
        return line >= span.StartLinePosition.Line + 1
               && line <= span.EndLinePosition.Line + 1
               && column >= span.StartLinePosition.Character + 1
               && column <= span.EndLinePosition.Character + 1;
    }

    private async Task<ISymbol?> FindSymbol(string type, string? member)
    {
        var candidates = new List<ISymbol>();
        foreach (var project in Workspace.CurrentSolution.Projects)
        {
            var candidatesInProject = await SymbolFinder.FindDeclarationsAsync(project, type, true, SymbolFilter.Type);
            candidates.AddRange(candidatesInProject);
        }

        if (member is null)
        {
            if (candidates is [var symbol])
            {
                return symbol;
            }

            throw new ArgumentException(
                $"Could not find type {type}; candidates are:\n{string.Join("\n", candidates.Select(c => c.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))}");
        }

        var memberCandidates = candidates.SelectMany(s => (s as ITypeSymbol)?.GetMembers() ?? ImmutableArray<ISymbol>.Empty)
            .Where(s => s.Name == member)
            .ToList();

        if (memberCandidates is [var memberSymbol])
        {
            return memberSymbol;
        }

        throw new ArgumentException(
            $"Could not find type {type}.{member}; candidates are:\n{string.Join("\n", memberCandidates.Select(c => c.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)))}");
    }

    [GeneratedRegex(@"^([^;]+);(\d)+:(\d)+$")]
    private static partial Regex CursorRegex();

    public static ICommandConfigurator Register(IConfigurator<RefactorCommandSettings> configurator)
    {
        return configurator.Register<RefactorSymbolCommand, RefactorSymbolSettings>(
            name: "symbol",
            description: "Run rewriters on a specific symbol",
            [
                ["refactor", "-s", "path/to/solution.sln", "symbol", "--cursor", "Foo.cs;2:14", "RenameSymbol(\"Bar\")"],
                ["refactor", "symbol", "--type", "Foo", "--member", "Bar", "RenameSymbol(\"BarAsync\")"]
            ]);
    }
}
