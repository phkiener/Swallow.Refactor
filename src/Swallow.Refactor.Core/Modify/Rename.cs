namespace Swallow.Refactor.Core.Rewriters;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class RenameExtensions
{
    public static SyntaxNode Rename(this SyntaxNode node, string identifer)
    {
        var token = SyntaxFactory.Identifier(identifer);

        return node switch
        {
            PropertyDeclarationSyntax property => property.Rename(token),
            MethodDeclarationSyntax method => method.Rename(token),
            EventDeclarationSyntax @event => @event.Rename(token),
            ParameterSyntax parameter => parameter.Rename(token),
            TypeDeclarationSyntax type => type.Rename(token),
            FieldDeclarationSyntax field => field.Rename(token),
            SimpleNameSyntax name => name.Rename(token),
            _ => node
        };
    }

    private static PropertyDeclarationSyntax Rename(this PropertyDeclarationSyntax property, SyntaxToken identifier)
    {
        return property.WithIdentifier(identifier.WithTriviaFrom(property.Identifier));
    }

    private static MethodDeclarationSyntax Rename(this MethodDeclarationSyntax method, SyntaxToken identifier)
    {
        return method.WithIdentifier(identifier.WithTriviaFrom(method.Identifier));
    }

    private static EventDeclarationSyntax Rename(this EventDeclarationSyntax @event, SyntaxToken identifier)
    {
        return @event.WithIdentifier(identifier.WithTriviaFrom(@event.Identifier));
    }

    private static ParameterSyntax Rename(this ParameterSyntax parameter, SyntaxToken identifier)
    {
        return parameter.WithIdentifier(identifier.WithTriviaFrom(parameter.Identifier));
    }

    private static TypeDeclarationSyntax Rename(this TypeDeclarationSyntax type, SyntaxToken identifier)
    {
        return type.WithIdentifier(identifier.WithTriviaFrom(type.Identifier));
    }

    private static FieldDeclarationSyntax Rename(this FieldDeclarationSyntax field, SyntaxToken identifier)
    {
        if (field.Declaration.Variables is [var variable])
        {
            return field.ReplaceNode(variable, variable.WithIdentifier(identifier.WithTriviaFrom(variable.Identifier)));
        }

        throw new NotSupportedException(
            $"Only fields defining exactly 1 variable can be renamed, but {field} declares {field.Declaration.Variables.Count}.");
    }

    private static SimpleNameSyntax Rename(this SimpleNameSyntax field, SyntaxToken identifier)
    {
        return field.WithIdentifier(identifier.WithTriviaFrom(field.Identifier));
    }
}
