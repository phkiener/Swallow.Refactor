namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Modify;

[Description("Optimize usings in a document, sorting them and removing unneccessary using directives.")]
public sealed class OptimizeUsings : SemanticDocumentRewriter
{
    private readonly List<string> forcedNamespaces;

    public OptimizeUsings([Description("List of namespaces to be kept, even if they are unused")] params string[] forcedNamespaces)
    {
        this.forcedNamespaces = forcedNamespaces.ToList();
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        var compilationUnit = (CompilationUnitSyntax)RootNode;
        var trivias = compilationUnit.Usings.Select(usingDeclaration => (usingDeclaration.GetLeadingTrivia(), usingDeclaration.GetTrailingTrivia()))
            .ToList();

        var usedNamespaces = GetUsedNamespaces(SemanticModel).Union(forcedNamespaces).ToHashSet();
        var adjustedUsings = compilationUnit.Usings
            .Where(
                u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword)
                     || u.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
                     || u.Alias is not null
                     || usedNamespaces.Contains(u.Name?.ToString() ?? "")
                     || usedNamespaces.Any(usedNamespace => u.Name?.ToString().EndsWith("::" + usedNamespace) ?? false))
            .OrderByDescending(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
            .ThenBy(
                u => u.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) ? 1 :
                    u.Alias is null ? 0 : 2)
            .ThenBy(x => x.Alias?.ToString())
            .ThenByDescending(x => x.Name?.ToString().StartsWith("System"))
            .ThenBy(x => x.Name?.ToString())
            .Select((ns, index) => BuildUsingFor(usingDirective: ns, trivias: trivias[index]))
            .ToList();

        documentEditor.RecordChange(node: compilationUnit, transformer: c => c.WithUsings(SyntaxFactory.List(adjustedUsings)));
    }

    private static IEnumerable<string> GetUsedNamespaces(SemanticModel semanticModel)
    {
        var rootNode = semanticModel.SyntaxTree.GetRoot();
        var allSymbols = rootNode.DescendantNodes()
            .Select(n => semanticModel.GetSymbolInfo(n))
            .Where(s => s.Symbol is not null)
            .Select(s => s.Symbol!)
            .ToHashSet(SymbolEqualityComparer.Default);

        return allSymbols.Select(s => s.ContainingNamespace.ToString()!).ToHashSet();
    }

    private static UsingDirectiveSyntax BuildUsingFor(
        UsingDirectiveSyntax usingDirective,
        (SyntaxTriviaList Leading, SyntaxTriviaList Trailing) trivias)
    {
        return usingDirective.WithLeadingTrivia(trivias.Leading).WithTrailingTrivia(trivias.Trailing);
    }
}
