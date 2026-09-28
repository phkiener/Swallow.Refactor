namespace Swallow.Refactor.Core.Modify;

using Testing.Assertion;
using Testing.Syntax;

internal sealed class MakeAsyncTest
{
    [Test]
    public void DoesNothing_WhenMethodIsAsyncTask()
    {
        var method = "public async Task MyAsyncMethod(int foo) { await Task.Delay(foo); }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(expected: method, actual: modifiedMethod);
    }

    [Test]
    public void DoesNothing_WhenMethodIsAsyncTaskOfType()
    {
        var method = "public async Task<int> MyAsyncMethod(int foo) { await Task.Delay(foo); return foo; }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(expected: method, actual: modifiedMethod);
    }

    [Test]
    public void AddsSuffix_WhenConfiguredMethodIsMissingAsyncSuffix()
    {
        var method = "public async Task<int> MyAsyncMethod(int foo) { await Task.Delay(foo); return foo; }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: true);
        SyntaxAssert.AreEqual(
            expected: "public async Task<int> MyAsyncMethodAsync(int foo) { await Task.Delay(foo); return foo; }",
            actual: modifiedMethod);
    }

    [Test]
    public void DoesNothing_WhenConfiguredToAddSuffixButItIsAlreadyPresent()
    {
        var method = "public async Task<int> MyAsyncMethodAsync(int foo) { await Task.Delay(foo); return foo; }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(
            expected: "public async Task<int> MyAsyncMethodAsync(int foo) { await Task.Delay(foo); return foo; }",
            actual: modifiedMethod);
    }

    [Test]
    public void TurnsMethodIntoAsyncTask_WhenMethodReturnsVoid()
    {
        var method = "public void MyMethod(int foo) { Console.WriteLine(foo); }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(expected: "public async Task MyMethod(int foo) { Console.WriteLine(foo); }", actual: modifiedMethod);
    }

    [Test]
    public void TurnsMethodIntoAsyncTaskOfType_WhenMethodReturnsType()
    {
        var method = "public int MyMethod(int foo) { Console.WriteLine(foo); return foo; }".ParseAs<MethodDeclarationSyntax>();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(expected: "public async Task<int> MyMethod(int foo) { Console.WriteLine(foo); return foo; }", actual: modifiedMethod);
    }

    [Test]
    public void MakesInterfaceMethodReturnTaskWithoutAddingAsyncKeyword()
    {
        var interfaceDeclaration = "public interface Foo {int MyMethod(int foo);}".ParseAs<InterfaceDeclarationSyntax>();
        var method = interfaceDeclaration.Members.OfType<MethodDeclarationSyntax>().Single();
        var modifiedMethod = method.MakeAsync(addSuffix: false);
        SyntaxAssert.AreEqual(expected: "Task<int> MyMethod(int foo);", actual: modifiedMethod);
    }
}
