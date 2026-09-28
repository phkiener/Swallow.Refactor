namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class MakeAsyncExtension
{
    private static readonly SyntaxToken Async = SyntaxFactory.Token(SyntaxKind.AsyncKeyword);

    public static MethodDeclarationSyntax MakeAsync(this MethodDeclarationSyntax method, bool addSuffix)
    {
        if (IsAsync(method) is false && ShouldMakeAsync(method))
        {
            var modifier = Async.WithTrailingTrivia(SyntaxFactory.ElasticSpace);
            method = method.AddModifiers(modifier);
        }

        if (ReturnsTask(method) is false)
        {
            var task = WrapInTask(method.ReturnType);
            method = method.WithReturnType(task);
        }

        if (addSuffix && method.Identifier.Text.EndsWith("Async") is false)
        {
            var identifier = AddSuffix(identifier: method.Identifier, suffix: "Async");
            method = method.WithIdentifier(identifier);
        }

        return method;
    }

    private static bool IsAsync(MethodDeclarationSyntax method)
    {
        return method.Modifiers.Any(m => m.ToString() == Async.ToString());
    }

    private static bool ShouldMakeAsync(MethodDeclarationSyntax method)
    {
        return method.Parent is not InterfaceDeclarationSyntax;
    }

    private static bool ReturnsTask(MethodDeclarationSyntax method)
    {
        var returnType = method.ReturnType.ToString();

        return returnType == "Task" || returnType.StartsWith("Task<");
    }

    private static TypeSyntax WrapInTask(TypeSyntax type)
    {
        var taskType = type.ToString() == "void" ? SyntaxFactory.ParseTypeName("Task") : SyntaxFactory.ParseTypeName($"Task<{type.ToString()}>");

        return taskType.WithLeadingTrivia(type.GetLeadingTrivia()).WithTrailingTrivia(type.GetTrailingTrivia());
    }

    private static SyntaxToken AddSuffix(SyntaxToken identifier, string suffix)
    {
        return SyntaxFactory.Identifier(identifier.Text + suffix)
            .WithLeadingTrivia(identifier.LeadingTrivia)
            .WithTrailingTrivia(identifier.TrailingTrivia);
    }
}
