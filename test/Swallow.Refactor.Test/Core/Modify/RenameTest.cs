namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis.CSharp;
using Rewriters;
using Testing.Assertion;
using Testing.Syntax;

internal sealed class RenameTest
{
    [Test]
    public void CanRenamePropertyDeclaration()
    {
        var property = "public string Foo { get; set; }".ParseAs<PropertyDeclarationSyntax>();
        var renamedProperty = property.Rename("Bar");
        SyntaxAssert.AreEqual(renamedProperty, "public string Bar { get; set; }");
    }

    [Test]
    public void CanRenameMethodDeclaration()
    {
        var method = "public void Foo() { }".ParseAs<MethodDeclarationSyntax>();
        var renamedMethod = method.Rename("Bar");
        SyntaxAssert.AreEqual(renamedMethod, "public void Bar() { }");
    }

    [Test]
    public void CanRenameEventDeclaration()
    {
        var @event = "public event EventHandler Foo { add; remove; }".ParseAs<EventDeclarationSyntax>();
        var renamedEvent = @event.Rename("Bar");
        SyntaxAssert.AreEqual(renamedEvent, "public event EventHandler Bar { add; remove; }");
    }

    [Test]
    public void CanRenameParameter()
    {
        var parameterList = "(string Foo)".ParseAs<ParameterListSyntax>();

        var renamedParameterList = parameterList.ReplaceNode(
                parameterList.Parameters.Single(),
                parameterList.Parameters.Single().Rename("Bar"));

        SyntaxAssert.AreEqual(renamedParameterList, "(string Bar)");
    }

    [Test]
    public void CanRenameClass()
    {
        var @class = "public class Foo { }".ParseAs<ClassDeclarationSyntax>();
        var renamedClass = @class.Rename("Bar");
        SyntaxAssert.AreEqual(renamedClass, "public class Bar { }");
    }

    [Test]
    public void CanRenameFieldWithOneDeclaration()
    {
        var field = "private string foo;".ParseAs<FieldDeclarationSyntax>();
        var renamedField = field.Rename("bar");
        SyntaxAssert.AreEqual(renamedField, "private string bar;");
    }

    [Test]
    public void ThrowsException_ForFieldWithMultipleDeclarations()
    {
        var field = "private string foo, bar;".ParseAs<FieldDeclarationSyntax>();
        Assert.Throws<NotSupportedException>(() => field.Rename("quuz"));
    }

    [Test]
    public void CanRenamePlainIdentifier()
    {
        var identifier = SyntaxFactory.IdentifierName("Foo");
        var renamedIdentifier = identifier.Rename("Bar");
        SyntaxAssert.AreEqual(renamedIdentifier, "Bar");
    }
}
