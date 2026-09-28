namespace Swallow.Refactor.Commands.Asyncify;

using Testing.Assertion;

internal sealed partial class AsyncifyCommandTest
{
    [Test]
    public async Task EmptyMethod_MethodIsMadeAsync()
    {
        var original = AddDocument("public static class Foo { public static void Bar() { } }");
        await RunAsyncifier("void Foo.Bar()");
        var modifiedDocument = await GetSourceTextAsync(original.Id);

        SyntaxAssert.AreEqual(
            actual: modifiedDocument,
            expected: "public static class Foo { public static async Task BarAsync() { } }");
    }

    [Test]
    public async Task MethodImplementsInterface_InterfaceMethodIsMadeAsyncAsWell()
    {
        var @interface = AddDocument("public interface IFoo { void Bar(); }");
        var implementation = AddDocument("public class Foo : IFoo { public void Bar(); } }");
        await RunAsyncifier("void Foo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(@interface.Id),
            expected: "public interface IFoo { Task BarAsync(); }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(implementation.Id),
            expected: "public class Foo : IFoo { public async Task BarAsync(); } }");
    }

    [Test]
    public async Task InterfaceMethodHasImplementations_ImplementationsAreMadeAsyncAsWell()
    {
        var @interface = AddDocument("public interface IFoo { void Bar(); }");
        var implementation = AddDocument("public class Foo : IFoo { public void Bar(); } }");
        await RunAsyncifier("void IFoo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(@interface.Id),
            expected: "public interface IFoo { Task BarAsync(); }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(implementation.Id),
            expected: "public class Foo : IFoo { public async Task BarAsync(); } }");
    }

    [Test]
    public async Task MethodOverridesBaseMethod_BaseIsMadeAsyncAsWell()
    {
        var baseClass = AddDocument("public class FooBase { public virtual void Bar(); }");
        var derivedClass = AddDocument("public class Foo : FooBase { public override void Bar(); } }");
        await RunAsyncifier("void Foo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(baseClass.Id),
            expected: "public class FooBase { public virtual async Task BarAsync(); }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(derivedClass.Id),
            expected: "public class Foo : FooBase { public override async Task BarAsync(); } }");
    }

    [Test]
    public async Task MethodHasOverrides_OverridesMadeAsyncAsWell()
    {
        var baseClass = AddDocument("public class FooBase { public virtual void Bar(); }");
        var derivedClass = AddDocument("public class Foo : FooBase { public override void Bar(); } }");
        await RunAsyncifier("void FooBase.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(baseClass.Id),
            expected: "public class FooBase { public virtual async Task BarAsync(); }");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(derivedClass.Id),
            expected: "public class Foo : FooBase { public override async Task BarAsync(); } }");
    }
}
