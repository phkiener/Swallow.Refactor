namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public static class AddUsingExtension
{
    public static CompilationUnitSyntax AddUsing(this CompilationUnitSyntax compilationUnit, string @namespace)
    {
        return compilationUnit.AddUsings(@namespace);
    }

    public static CompilationUnitSyntax AddUsings(this CompilationUnitSyntax compilationUnit, params string[] namespaces)
    {
        var namespacesToAdd = namespaces.Where(@namespace => compilationUnit.Usings.Any(u => u.Name?.ToString() == @namespace) is false).ToList();
        if (namespacesToAdd.Count == 0)
        {
            return compilationUnit;
        }

        var modifiedUsings = compilationUnit.Usings.AppendUsings(namespacesToAdd);

        return compilationUnit.WithUsings(modifiedUsings);
    }

    private static SyntaxList<UsingDirectiveSyntax> AppendUsings(this SyntaxList<UsingDirectiveSyntax> existingUsings, IEnumerable<string> namespaces)
    {
        var usings = new SyntaxList<UsingDirectiveSyntax>();
        foreach (var usingDirective in existingUsings)
        {
            usings = usings.Add(usingDirective.WithTrailingTrivia(ElasticLineFeed));
        }

        foreach (var @namespace in namespaces)
        {
            var usingDirective = UsingDirective(IdentifierName(@namespace).WithLeadingTrivia(ElasticSpace));
            usings = usings.Add(usingDirective.WithTrailingTrivia(ElasticLineFeed));
        }

        if (existingUsings.Count == 0)
        {
            usings = usings.Replace(nodeInList: usings.Last(), newNode: usings.Last().WithTrailingTrivia(ElasticLineFeed, ElasticLineFeed));
        }

        return usings;
    }
}
