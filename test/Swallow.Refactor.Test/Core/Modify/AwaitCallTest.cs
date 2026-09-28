namespace Swallow.Refactor.Core.Modify;

using Testing.Assertion;
using Testing.Syntax;

internal sealed class AwaitCallTest
{
    [Test]
    public void DoesNothing_WhenCallIsNotAwaitable()
    {
        var invocation = "Console.WriteLine(foo)".ParseAs<InvocationExpressionSyntax>();
        var modifiedInvocation = invocation.AwaitCall();
        SyntaxAssert.AreEqual(expected: invocation, actual: modifiedInvocation);
    }

    [Test]
    public void DoesNothing_WhenCallIsAlreadyAwaited()
    {
        var method = "await Console.WriteLineAsync(foo)".ParseAs<ExpressionSyntax>();
        var invocation = method.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
        var modifiedInvocation = invocation.AwaitCall();
        SyntaxAssert.AreEqual(expected: invocation, actual: modifiedInvocation);
    }

    [Test]
    public void AwaitsCall_WhenGetAwaiterGetResultIsCalled()
    {
        var invocation = "Console.WriteLineAsync(foo).GetAwaiter().GetResult()".ParseAs<InvocationExpressionSyntax>();
        var modifiedInvocation = invocation.AwaitCall();
        SyntaxAssert.AreEqual(expected: "await Console.WriteLineAsync(foo)", actual: modifiedInvocation);
    }

    [Test]
    public void AwaitsCallInParens_WhenGetAwaiterGetResultIsCalledAndChainedFurther()
    {
        var outerInvocation = "Task.FromResult(foo).GetAwaiter().GetResult().ToString()".ParseAs<InvocationExpressionSyntax>();
        var innerInvocation = GetInnerExpression(outerInvocation);

        var modifiedInvocation = innerInvocation!.AwaitCall();
        SyntaxAssert.AreEqual(expected: "(await Task.FromResult(foo))", actual: modifiedInvocation);
    }

    private static InvocationExpressionSyntax? GetInnerExpression(InvocationExpressionSyntax invocation)
    {
        var member = invocation.Expression as MemberAccessExpressionSyntax;

        return member?.Expression as InvocationExpressionSyntax;
    }
}
