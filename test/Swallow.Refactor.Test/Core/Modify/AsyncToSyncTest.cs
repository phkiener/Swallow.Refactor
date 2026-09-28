namespace Swallow.Refactor.Core.Modify;

using Testing.Assertion;
using Testing.Syntax;

internal sealed class AsyncToSyncTest
{
    [Test]
    public void AddsGetAwaiterGetResultToMethodInvocation()
    {
        var invocation = "Task.Delay(500)".ParseAs<InvocationExpressionSyntax>();
        var modifiedInvocation = invocation.AsyncToSync();
        SyntaxAssert.AreEqual(expected: "Task.Delay(500).GetAwaiter().GetResult()", actual: modifiedInvocation);
    }
}
