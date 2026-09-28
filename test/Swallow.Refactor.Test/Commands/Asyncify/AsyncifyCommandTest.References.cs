namespace Swallow.Refactor.Commands.Asyncify;

using Testing.Assertion;

internal sealed partial class AsyncifyCommandTest
{
    [Test]
    public async Task MethodIsInvoked_MakesInvokingMethodAsync()
    {
        var callee = AddDocument("public class cA { public void A() { } }");
        var caller = AddDocument("public class cB { public void B() { new cA().A(); } }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(callee.Id),
            expected: "public class cA { public async Task AAsync() { } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(caller.Id),
            expected: "public class cB { public async Task BAsync() { await new cA().AAsync(); } }");
    }

    [Test]
    public async Task MethodIsReferencedInNameof_RenamesReference()
    {
        var callee = AddDocument("public class cA { public void A() { } }");
        var caller = AddDocument("public class cB { public string B => nameof(cA.A); }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(callee.Id),
            expected: "public class cA { public async Task AAsync() { } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(caller.Id),
            expected: "public class cB { public string B => nameof(cA.AAsync); }");
    }

    [Test]
    public async Task MethodIsReferencedInProperty_UsesGetAwaiterGetResult()
    {
        var callee = AddDocument("public class cA { public void A() { } }");
        var caller = AddDocument("public class cB { public string B => new cA().A(); }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(callee.Id),
            expected: "public class cA { public async Task AAsync() { } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(caller.Id),
            expected: "public class cB { public string B => new cA().AAsync().GetAwaiter().GetResult(); }");
    }

    [Test]
    public async Task AsyncifyingWorksForChainedReferences()
    {
        var first = AddDocument("public class cA { public void A() { } }");
        var second = AddDocument("public class cB { public void B() { new cA().A(); } }");
        var third = AddDocument("public class cC { public void C() { new cB().B(); } }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(first.Id),
            expected: "public class cA { public async Task AAsync() { } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(second.Id),
            expected: "public class cB { public async Task BAsync() { await new cA().AAsync(); } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(third.Id),
            expected: "public class cC { public async Task CAsync() { await new cB().BAsync(); } }");
    }

    [Test]
    public async Task CanHandleDirectCircularReference()
    {
        var first = AddDocument("public class cA { public void A() { new cB().B(); } }");
        var second = AddDocument("public class cB { public void B() { new cA().A(); } }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(first.Id),
            expected: "public class cA { public async Task AAsync() { await new cB().BAsync(); } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(second.Id),
            expected: "public class cB { public async Task BAsync() { await new cA().AAsync(); } }");
    }

    [Test]
    public async Task CanHandleIndirectCircularReference()
    {
        var first = AddDocument("public class cA { public void A() { new cC().C(); } }");
        var second = AddDocument("public class cB { public void B() { new cA().A(); } }");
        var third = AddDocument("public class cC { public void C() { new cB().B(); } }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(first.Id),
            expected: "public class cA { public async Task AAsync() { await new cC().CAsync(); } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(second.Id),
            expected: "public class cB { public async Task BAsync() { await new cA().AAsync(); } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(third.Id),
            expected: "public class cC { public async Task CAsync() { await new cB().BAsync(); } }");
    }

    [Test]
    public async Task AsyncifyingSkipsIfAsyncOverloadIsAlreadyAvailable()
    {
        var first = AddDocument("public abstract class AbstractBase { public virtual void A() { }; public virtual async Task AAsync() { }; }");
        var second = AddDocument("public class ImplementationUsingBase : AbstractBase { public override void A() { new SomeSyncStuff().C(); } }");
        var third = AddDocument("public class SomeSyncStuff { public void C() { } }");

        await RunAsyncifier("void C()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(first.Id),
            expected: "public abstract class AbstractBase { public virtual void A() { }; public virtual async Task AAsync() { }; }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(second.Id),
            expected: "public class ImplementationUsingBase : AbstractBase { public override async Task AAsync() { await new SomeSyncStuff().CAsync(); } }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(third.Id),
            expected: "public class SomeSyncStuff { public async Task CAsync() { } }");
    }

    [Test]
    public async Task CanHandleRecursion()
    {
        var loop = AddDocument("public class cA { public void A() { A(); } }");

        await RunAsyncifier("void cA.A()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(loop.Id),
            expected: "public class cA { public async Task AAsync() { await AAsync(); } }");
    }
}
