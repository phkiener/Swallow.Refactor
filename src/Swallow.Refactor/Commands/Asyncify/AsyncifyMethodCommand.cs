namespace Swallow.Refactor.Commands.Asyncify;

using Core;
using Core.Modify;
using Core.Query;
using Execution;
using Execution.Registration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Spectre.Console.Cli;

public sealed class AsyncifyMethodCommand : ProgressCommand<AsyncifyMethodSettings>, IRegisterableCommand
{
    protected override async Task RunAsync(AsyncifyMethodSettings settings)
    {
        var project = Workspace.CurrentSolution.OpenProject(settings.Project);
        var startingSymbol = await RunAsync(name: "Finding starting symbol", func: async () => await project.FindSymbol(settings.Method));

        var (declarations, references) = await RunAsync(
            name: "Collecting symbols to adjust",
            func: async () => await CollectSymbolsAsync(startingSymbol, Workspace.CurrentSolution));

        var solutionEditor = new SolutionEditor(Workspace.CurrentSolution);
        await ProcessAsync(
            declaration => $"Processing declaration {declaration.Name}",
            declarations,
            async dec => await ProcessDeclaration(dec, solutionEditor));

        await ProcessAsync(
            usage => $"Processing usage in {usage.Document.FilePath}",
            references,
            async usage => await ProcessUsage(usage, solutionEditor));

        var changedSolution = solutionEditor.GetChangedSolution();
        Workspace.TryApplyChanges(changedSolution);
    }

    private static async Task<CollectedSymbols> CollectSymbolsAsync(IMethodSymbol startingSymbol, Solution solution)
    {
        var declarations = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var references = new HashSet<ReferenceLocation>();

        var symbolQueue = new Queue<IMethodSymbol>(new[] { startingSymbol });
        while (symbolQueue.TryDequeue(out var symbol))
        {
            if (declarations.Contains(symbol))
            {
                continue;
            }

            var membersOfContainingType = symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().ToList();
            if (HasAsyncOverload(symbol, membersOfContainingType))
            {
                continue;
            }

            var baseType = symbol.ContainingType.BaseType;
            while (baseType is not null)
            {
                membersOfContainingType.AddRange(baseType.GetMembers().OfType<IMethodSymbol>());
                baseType = baseType.BaseType;
            }

            if (HasAsyncOverload(symbol, membersOfContainingType))
            {
                declarations.Add(symbol.OriginalDefinition);

                continue;
            }

            var currentReferences = (await SymbolFinder.FindReferencesAsync(symbol, solution)).ToList();
            declarations.UnionWith(currentReferences.Select(r => r.Definition));

            foreach (var reference in currentReferences.SelectMany(r => r.Locations))
            {
                references.Add(reference);

                var semanticModel = await reference.Document.GetSemanticModelAsync();
                if (semanticModel?.GetEnclosingSymbol(reference.Location.SourceSpan.Start) is IMethodSymbol { IsAsync: false } enclosingSymbol)
                {
                    symbolQueue.Enqueue(enclosingSymbol);
                }
            }
        }

        return new(declarations, references);
    }

    private static bool HasAsyncOverload(IMethodSymbol syncMethod, IReadOnlyList<IMethodSymbol> asyncMethodCandidates)
    {
        var nameMatches = asyncMethodCandidates.Where(m => m.Name == syncMethod.Name + "Async").ToList();
        if (!nameMatches.Any())
        {
            return false;
        }

        foreach (var nameMatch in nameMatches)
        {
            if (syncMethod.Parameters.Length != nameMatch.Parameters.Length
                && syncMethod.Parameters.Length + 1 != nameMatch.Parameters.Length) // +1 to account for the cancellation token
            {
                continue;
            }

            if (syncMethod.Parameters.Length != nameMatch.Parameters.Length
                && nameMatch.Parameters.Last().Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) != nameof(CancellationToken))
            {
                continue;
            }

            var isMatch = syncMethod.Parameters.Zip(nameMatch.Parameters)
                .All(tuple => tuple.First.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                              == tuple.Second.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            if (isMatch)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task ProcessDeclaration(ISymbol declaration, SolutionEditor solutionEditor)
    {
        foreach (var location in declaration.Locations.Where(l => l.SourceTree is not null))
        {
            var documentIds = solutionEditor.OriginalSolution.GetDocumentIdsWithFilePath(location.SourceTree!.FilePath);
            foreach (var document in documentIds)
            {
                var editor = await solutionEditor.GetDocumentEditorAsync(document);
                var node = editor.OriginalRoot.FindNode(location.SourceSpan);

                if (node is MethodDeclarationSyntax methodDeclaration)
                {
                    editor.RecordChange(methodDeclaration, n => n.MakeAsync(addSuffix: true));

                    foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(i => i.IsAwaitable()))
                    {
                        editor.RecordChange(invocation, n => n.AwaitCall());
                    }
                }
            }
        }
    }

    private static async Task ProcessUsage(ReferenceLocation usage, SolutionEditor solutionEditor)
    {
        var documentEditor = await solutionEditor.GetDocumentEditorAsync(usage.Document.Id);
        var node = documentEditor.OriginalRoot.FindNode(usage.Location.SourceSpan);

        if (node is IdentifierNameSyntax identifier)
        {
            var parentNode = identifier.Parent;

            var leadingTrivia = new SyntaxTriviaList();
            if (parentNode is not null && parentNode.HasLeadingTrivia)
            {
                leadingTrivia = parentNode.GetLeadingTrivia();
                documentEditor.RecordChange(parentNode, n => n.WithoutLeadingTrivia());
            }

            documentEditor.RecordChange(
                identifier,
                n => n.WithIdentifier(SyntaxFactory.Identifier(n.Identifier.Text + "Async")).WithoutLeadingTrivia());

            if (IsInvoked(identifier, out var invocation))
            {
                if (CanAwait(invocation))
                {
                    documentEditor.RecordChange(invocation, n => AwaitExpression(n, leadingTrivia));
                }
                else
                {
                    documentEditor.RecordChange(invocation, n => n.AsyncToSync());
                }
            }
        }
    }

    private static bool IsInvoked(IdentifierNameSyntax identifier, out InvocationExpressionSyntax invocation)
    {
        var parent = identifier.Parent;
        while (parent is not null or BlockSyntax or LambdaExpressionSyntax)
        {
            if (parent is InvocationExpressionSyntax { Expression: not IdentifierNameSyntax { Identifier.Text: "nameof" or "typeof" } } invocationExpression)
            {
                invocation = invocationExpression;
                return true;
            }

            parent = parent.Parent;
        }

        invocation = default!;
        return false;
    }

    private static bool CanAwait(InvocationExpressionSyntax invocation)
    {
        return HasParent(invocation, n => n is MethodDeclarationSyntax);
    }

    private static AwaitExpressionSyntax AwaitExpression(InvocationExpressionSyntax invocation, SyntaxTriviaList leadingTrivia)
    {
        return SyntaxFactory.AwaitExpression(SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
            .WithTrailingTrivia(SyntaxFactory.ElasticSpace), invocation)
            .WithLeadingTrivia(leadingTrivia);
    }

    private static bool HasParent(SyntaxNode node, Predicate<SyntaxNode> predicate)
    {
        var current = node.Parent;
        while (current is not null)
        {
            if (predicate(current))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private sealed record CollectedSymbols(IReadOnlyCollection<ISymbol> Declarations, IReadOnlyCollection<ReferenceLocation> References);

    public static ICommandConfigurator RegisterWith(IConfigurator configurator)
    {
        return configurator.Register<AsyncifyMethodCommand>(
            name: "asyncify",
            description: "Asyncify a method and its callers, awaiting whatever invocations can be awaited.",
            examples: new[]
            {
                "asyncify",
                "My.Project",
                "void Foo.Bar(int foobar)"
            });
    }
}
