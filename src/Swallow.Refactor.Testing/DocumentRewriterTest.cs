namespace Swallow.Refactor.Testing;

using Abstractions.Rewriting;

public abstract class DocumentRewriterTest : RoslynTest
{
    protected abstract IDocumentRewriter DocumentRewriter { get; }

    protected async Task<string> RunRewriterAsync(SourceText sourceCode)
    {
        var document = AddDocument(sourceCode);
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var syntaxTree = await documentEditor.OriginalDocument.GetSyntaxTreeAsync();
        await DocumentRewriter.RunAsync(documentEditor: documentEditor, syntaxTree: syntaxTree!);

        return await GetFormattedSourceCodeAsync(documentEditor.GetChangedDocument());
    }
}
