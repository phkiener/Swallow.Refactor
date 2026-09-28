namespace Swallow.Refactor.Core.Query;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

public static class ProjectQueries
{
    public static async Task<IMethodSymbol> FindSymbol(this Project project, string declaration)
    {
        var memberDeclarationSyntax = SyntaxFactory.ParseMemberDeclaration(declaration);
        if (memberDeclarationSyntax is not MethodDeclarationSyntax methodDeclaration)
        {
            throw new InvalidOperationException($"Cannot parse '{declaration}'.");
        }

        var symbolCandidates = await SymbolFinder.FindDeclarationsAsync(
            project: project,
            name: methodDeclaration.Identifier.Text,
            ignoreCase: false,
            filter: SymbolFilter.Member);

        var filters = new[] { FilterUsingFullyQualifiedIdentifier, FilterUsingReturnType, FilterUsingParameterList };
        var symbols = filters.Aggregate(
                seed: symbolCandidates.OfType<IMethodSymbol>(),
                func: (symbols, filter) => filter.Invoke(arg1: symbols, arg2: methodDeclaration))
            .ToList();

        return symbols switch
        {
            { Count: 0 } => throw new KeyNotFoundException($"No symbol found matching '{declaration}'."),
            { Count: > 1 } => throw new InvalidOperationException($"Too many symbols found matching '{declaration}'."),
            _ => symbols.Single()
        };
    }

    private static IEnumerable<IMethodSymbol> FilterUsingFullyQualifiedIdentifier(
        IEnumerable<IMethodSymbol> methodCandidates,
        MethodDeclarationSyntax methodDeclaration)
    {
        foreach (var candidate in methodCandidates)
        {
            if (methodDeclaration.ExplicitInterfaceSpecifier is not null)
            {
                var expectedContainingType = methodDeclaration.ExplicitInterfaceSpecifier.Name.ToString();
                var actualContainingType = candidate.ContainingType.Name;
                if (expectedContainingType == actualContainingType)
                {
                    yield return candidate;
                }
            }
            else
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<IMethodSymbol> FilterUsingReturnType(
        IEnumerable<IMethodSymbol> methodCandidates,
        MethodDeclarationSyntax methodDeclaration)
    {
        foreach (var candidate in methodCandidates)
        {
            var expectedReturnType = methodDeclaration.ReturnType.ToString();
            if (expectedReturnType == candidate.ReturnType.ToString())
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<IMethodSymbol> FilterUsingParameterList(
        IEnumerable<IMethodSymbol> methodCandidates,
        MethodDeclarationSyntax methodDeclaration)
    {
        foreach (var candidate in methodCandidates)
        {
            var expectedParameters = methodDeclaration.ParameterList.Parameters;
            var actualParameters = candidate.Parameters;
            if (expectedParameters.Count != actualParameters.Length)
            {
                continue;
            }

            var allParametersMatch = expectedParameters.Zip(actualParameters).All(tuple => AreEquivalent(syntax: tuple.First, symbol: tuple.Second));
            if (allParametersMatch)
            {
                yield return candidate;
            }
        }

        yield break;

        static bool AreEquivalent(ParameterSyntax syntax, IParameterSymbol symbol)
        {
            return syntax.Identifier.Text == symbol.Name
                   && syntax.Type?.ToString() == symbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }
    }
}
