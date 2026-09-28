namespace Swallow.Refactor.Core.Modify;

using Testing.Assertion;
using Testing.Syntax;

internal sealed class AddUsingTest
{
    [Test]
    public void DoesNothing_WhenUsingAlreadyExists()
    {
        var compilationUnit = "using System; namespace Foo;".ParseAs<CompilationUnitSyntax>();
        var modifiedCompilationUnit = compilationUnit.AddUsing("System");
        SyntaxAssert.AreEqual(expected: compilationUnit, actual: modifiedCompilationUnit);
    }

    [Test]
    public void AddsSingleUsingAtEnd_WhenAUsingIsAlreadyPresent()
    {
        var compilationUnit = """
            using System;

            namespace Foo;
            """.ParseAs<CompilationUnitSyntax>();

        var modifiedCompilationUnit = compilationUnit.AddUsing("System.Threading.Tasks");
        SyntaxAssert.AreEqual(
            actual: modifiedCompilationUnit,
            expected: """
            using System;
            using System.Threading.Tasks;

            namespace Foo;
            """);
    }

    [Test]
    public void AddsSingleUsing_WhenNoUsingsArePresent()
    {
        var compilationUnit = "namespace Foo;".ParseAs<CompilationUnitSyntax>();
        var modifiedCompilationUnit = compilationUnit.AddUsing("System");
        SyntaxAssert.AreEqual(
            actual: modifiedCompilationUnit,
            expected: """
            using System;

            namespace Foo;
            """);
    }

    [Test]
    public void AddsMultipleUsings_WhenNoUsingsArePresent()
    {
        var compilationUnit = "namespace Foo;".ParseAs<CompilationUnitSyntax>();
        var modifiedCompilationUnit = compilationUnit.AddUsings("System", "System.Threading.Tasks");
        SyntaxAssert.AreEqual(
            actual: modifiedCompilationUnit,
            expected: """
            using System;
            using System.Threading.Tasks;

            namespace Foo;
            """);
    }

    [Test]
    public void AddsNotYetPresentUsingsAtEnd_WhenSomeUsingsArePresent()
    {
        var compilationUnit = "namespace Foo;".ParseAs<CompilationUnitSyntax>();
        var modifiedCompilationUnit = compilationUnit.AddUsings("System", "System.IO", "System.Threading.Tasks");
        SyntaxAssert.AreEqual(
            actual: modifiedCompilationUnit,
            expected: """
            using System;
            using System.IO;
            using System.Threading.Tasks;

            namespace Foo;
            """);
    }
}
