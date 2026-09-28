namespace Swallow.Refactor.Core.Modify;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class AwaitCallsExtension
{
    public static bool IsAwaitable(this InvocationExpressionSyntax invocation)
    {
        return invocation.IsAwaitable(out _);
    }

    public static ExpressionSyntax AwaitCall(this InvocationExpressionSyntax invocation)
    {
        if (!invocation.IsAwaitable(out var getAwaiter))
        {
            return invocation;
        }

        var keyword = SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
            .WithLeadingTrivia(getAwaiter.Expression.GetLeadingTrivia())
            .WithTrailingTrivia(SyntaxFactory.ElasticSpace);

        var calledExpression = getAwaiter.Expression.WithoutLeadingTrivia();
        var awaitExpression = SyntaxFactory.AwaitExpression(awaitKeyword: keyword, expression: calledExpression)
            .WithoutTrailingTrivia();

        return NeedsParentheses(invocation: invocation) ? SyntaxFactory.ParenthesizedExpression(awaitExpression) : awaitExpression;
    }

    private static bool IsAwaitable(
        this InvocationExpressionSyntax invocation,
        [NotNullWhen(true)] out MemberAccessExpressionSyntax? getAwaiterAccess)
    {
        if (!IsGetResult(invocationExpressionSyntax: invocation, getResultAccess: out var getResult))
        {
            getAwaiterAccess = null;
            return false;
        }

        if (!IsGetAwaiter(invocationExpressionSyntax: getResult.Expression as InvocationExpressionSyntax, getAwaiterAccess: out var getAwaiter))
        {
            getAwaiterAccess = null;
            return false;
        }

        getAwaiterAccess = getAwaiter;

        return true;
    }

    private static bool IsGetAwaiter(
        InvocationExpressionSyntax? invocationExpressionSyntax,
        [NotNullWhen(true)] out MemberAccessExpressionSyntax? getAwaiterAccess)
    {
        if (invocationExpressionSyntax?.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            getAwaiterAccess = memberAccess;

            return memberAccess.Name.Identifier.Text == "GetAwaiter" && invocationExpressionSyntax.ArgumentList.Arguments.Count == 0;
        }

        getAwaiterAccess = null;

        return false;
    }

    private static bool IsGetResult(
        InvocationExpressionSyntax? invocationExpressionSyntax,
        [NotNullWhen(true)] out MemberAccessExpressionSyntax? getResultAccess)
    {
        if (invocationExpressionSyntax?.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            getResultAccess = memberAccess;

            return memberAccess.Name.Identifier.Text == "GetResult" && invocationExpressionSyntax.ArgumentList.Arguments.Count == 0;
        }

        getResultAccess = null;

        return false;
    }

    private static bool NeedsParentheses(InvocationExpressionSyntax invocation)
    {
        return invocation.Parent is not StatementSyntax and not ArgumentSyntax and not null;
    }
}
