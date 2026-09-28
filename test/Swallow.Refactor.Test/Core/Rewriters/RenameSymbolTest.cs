namespace Swallow.Refactor.Core.Rewriters;

using Abstractions.Rewriting;
using Microsoft.CodeAnalysis.FindSymbols;
using Testing;
using Testing.Assertion;

internal sealed class RenameSymbolTest : TargetedRewriterTest
{
    protected override ITargetedRewriter TargetedRewriter => new RenameSymbol("Bar");

    [Test]
    public async Task DeclarationOfSymbolIsAdjusted()
    {
        var document = AddDocument("public record Foo(string Foo);");
        var candidates = await SymbolFinder.FindDeclarationsAsync(project: CurrentProject, name: "Foo", ignoreCase: true, filter: SymbolFilter.Member);

        await RunRewriterAsync(symbol: candidates.Single());

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(document.Id)!.GetTextAsync(),
            expected: "public record Foo(string Bar);");
    }

    [Test]
    public async Task AllDeclarationsOfPartialClassAreAdjusted()
    {
        var first = AddDocument("public partial class Foo { public partial void Foo(); }");
        var second = AddDocument("public partial class Foo { public partial void Foo() { } }");

        var candidates = await SymbolFinder.FindDeclarationsAsync(project: CurrentProject, name: "Foo", ignoreCase: true, filter: SymbolFilter.Type);
        await RunRewriterAsync(symbol: candidates.Single());

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(first.Id)!.GetTextAsync(),
            expected: "public partial class Bar { public partial void Foo(); }");

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(second.Id)!.GetTextAsync(),
            expected: "public partial class Bar { public partial void Foo() { } }");
    }

    [Test]
    public async Task UsageOfPropertyIsAdjusted()
    {
        var first = AddDocument("public class Foo { public int Bar() => new Bar().Foo; }");
        var second = AddDocument("public class Bar { public int Foo { get; set; } = 0; }");

        var candidates = await SymbolFinder.FindDeclarationsAsync(project: CurrentProject, name: "Foo", ignoreCase: true, filter: SymbolFilter.Member);
        await RunRewriterAsync(symbol: candidates.Single());

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(first.Id)!.GetTextAsync(),
            expected: "public class Foo { public int Bar() => new Bar().Bar; }");

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(second.Id)!.GetTextAsync(),
            expected: "public class Bar { public int Bar { get; set; } = 0; }");
    }

    [Test]
    public async Task ImplementationsOfInterfaceMethodAreRenamed()
    {
        var @interface = AddDocument("public interface Foo { int Foo(); }");
        var implicitImplementation = AddDocument("public class ImplicitFoo : Foo { public int Foo() => 0; }");
        var explicitImplementation = AddDocument("public class ExplicitFoo : Foo { int Foo.Foo() => 0; }");

        var candidates = await SymbolFinder.FindDeclarationsAsync(project: CurrentProject, name: "Foo", ignoreCase: true, filter: SymbolFilter.Member);
        await RunRewriterAsync(symbol: candidates.Single(c => c.ContainingType.TypeKind is TypeKind.Interface));

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(@interface.Id)!.GetTextAsync(),
            expected: "public interface Foo { int Bar(); }");

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(implicitImplementation.Id)!.GetTextAsync(),
            expected: "public class ImplicitFoo : Foo { public int Bar() => 0; }");

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(explicitImplementation.Id)!.GetTextAsync(),
            expected: "public class ExplicitFoo : Foo { int Foo.Bar() => 0; }");
    }
}
