namespace Swallow.Refactor.Commands.RefactorSymbol;

using Core.Rewriters;
using Refactor.Symbol;
using Testing.Assertion;
using Testing.Commands;

public sealed class RefactorSymbolCommandTest : CommandTest<RefactorSymbolCommand, RefactorSymbolSettings>
{
    protected override RefactorSymbolCommand Command { get; } = new();

    [SetUp]
    public void SetUp()
    {
        Registry.IncludeTargetedRewriter<RenameSymbol>();
    }

    [Test]
    public async Task FindsSymbolUnderCursor()
    {
        var file = AddDocument("File.cs", """
            namespace SomeFile;

            public sealed record SomeRecord(int Id, string Name);
            """);

        await RunCommand(new() { Cursor = "File.cs;3:25", RewriterSpecs = ["RenameSymbol(SomeModifiedRecord)"] });
        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
                      namespace SomeFile;

                      public sealed record SomeModifiedRecord(int Id, string Name);
                      """);
    }

    [Test]
    public async Task FindsSymbolViaType()
    {
        var file = AddDocument("File.cs", """
                                          namespace SomeFile;

                                          public sealed record SomeRecord(int Id, string Name);
                                          """);

        await RunCommand(new() { Type = "SomeRecord", RewriterSpecs = ["RenameSymbol(SomeModifiedRecord)"] });
        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
                      namespace SomeFile;

                      public sealed record SomeModifiedRecord(int Id, string Name);
                      """);
    }

    [Test]
    public async Task FindsSymbolViaTypeAndMember()
    {
        var file = AddDocument("File.cs", """
                                          namespace SomeFile;

                                          public sealed record SomeRecord(int Id, string Name);
                                          """);

        await RunCommand(new() { Type = "SomeRecord", Member = "Name", RewriterSpecs = ["RenameSymbol(RenamedMember)"] });
        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
                      namespace SomeFile;

                      public sealed record SomeRecord(int Id, string RenamedMember);
                      """);
    }

    [Test]
    public async Task RewritersCanBeChained()
    {
        var file = AddDocument("File.cs", """
                                          namespace SomeFile;

                                          public sealed record SomeRecord(int Id, string Name);
                                          """);

        await RunCommand(new() { Type = "SomeRecord", Member = "Name", RewriterSpecs = ["RenameSymbol(RenamedMember)", "RenameSymbol(RenamedAgain)"] });
        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
                      namespace SomeFile;

                      public sealed record SomeRecord(int Id, string RenamedAgain);
                      """);
    }
}
