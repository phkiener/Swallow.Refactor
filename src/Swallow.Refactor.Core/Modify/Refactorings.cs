namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

public static class Refactorings
{
    public static void RenameReferencesTo(this DocumentEditor documentEditor, ISymbol symbol, string name, SemanticModel semanticModel)
    {
        var rootNode = documentEditor.OriginalRoot;
        var identifierNodes = rootNode.DescendantNodes(_ => true).OfType<IdentifierNameSyntax>();
        foreach (var identifier in identifierNodes)
        {
            var identifierSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (SymbolEqualityComparer.Default.Equals(x: identifierSymbol, y: symbol))
            {
                var changedIdentifier = SyntaxFactory.Identifier(name)
                    .WithLeadingTrivia(identifier.GetLeadingTrivia())
                    .WithTrailingTrivia(identifier.GetTrailingTrivia());

                documentEditor.RecordChange(node: identifier, transformer: i => i.WithIdentifier(changedIdentifier));
            }
        }
    }

    public static void ChangeParameter(this DocumentEditor documentEditor, ParameterSyntax parameter, string typeName, string variableName)
    {
        var type = SyntaxFactory.IdentifierName(typeName)
            .WithLeadingTrivia(parameter.Type!.GetLeadingTrivia())
            .WithTrailingTrivia(parameter.Type!.GetTrailingTrivia());

        var identifier = SyntaxFactory.Identifier(variableName)
            .WithLeadingTrivia(parameter.Identifier.LeadingTrivia)
            .WithTrailingTrivia(parameter.Identifier.TrailingTrivia);

        documentEditor.RecordChange(node: parameter, transformer: p => p.WithType(type).WithIdentifier(identifier));
    }

    public static void ChangeField(this DocumentEditor documentEditor, FieldDeclarationSyntax field, string typeName, string variableName)
    {
        var type = SyntaxFactory.IdentifierName(typeName)
            .WithLeadingTrivia(field.Declaration.Type.GetLeadingTrivia())
            .WithTrailingTrivia(field.Declaration.Type.GetTrailingTrivia());

        var identifier = SyntaxFactory.VariableDeclarator(variableName);
        var declaration = field.Declaration.WithType(type).WithVariables(SyntaxFactory.SingletonSeparatedList(identifier));
        documentEditor.RecordChange(node: field, transformer: f => f.WithDeclaration(declaration));
    }

    public static void ReplaceNodeWithCode(this DocumentEditor documentEditor, SyntaxNode node, string expression)
    {
        var replacementExpression = SyntaxFactory.ParseExpression(expression);
        documentEditor.ReplaceNode(node: node, newNode: replacementExpression);
    }
}
