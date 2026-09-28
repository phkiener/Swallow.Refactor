namespace Swallow.Refactor.Core.Query;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class DocumentQueries
{
    public static async Task<MethodDeclarationSyntax> GetMethodAsync(this Document document, string methodName)
    {
        var rootNode = await document.GetSyntaxRootAsync();

        return rootNode!.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == methodName);
    }

    public static async Task<InvocationExpressionSyntax> GetInvocation(this Document document, string invokedMethodName)
    {
        var rootNode = await document.GetSyntaxRootAsync();

        return rootNode!.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single(i => i.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == invokedMethodName);
    }

    public static async Task<CompilationUnitSyntax> GetCompilationUnit(this Document document)
    {
        var rootNode = await document.GetSyntaxRootAsync();

        return (CompilationUnitSyntax)rootNode!;
    }
}
