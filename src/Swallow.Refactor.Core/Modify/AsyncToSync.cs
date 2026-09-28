namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class AsyncToSyncExtension
{
    public static InvocationExpressionSyntax AsyncToSync(this InvocationExpressionSyntax invocation)
    {
        return Invoke(methodName: "GetResult", expression: Invoke(methodName: "GetAwaiter", expression: invocation));
    }

    private static InvocationExpressionSyntax Invoke(string methodName, ExpressionSyntax expression)
    {
        var memberAccess = SyntaxFactory.MemberAccessExpression(
            kind: SyntaxKind.SimpleMemberAccessExpression,
            expression: expression,
            name: SyntaxFactory.IdentifierName(methodName));

        return SyntaxFactory.InvocationExpression(memberAccess);
    }
}
