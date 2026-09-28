namespace Swallow.Refactor.Abstractions.Rewriting;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

/// <summary>
///     A base rewriter that can be used when no semantic analysis is neccessary.
/// </summary>
public abstract class SyntaxDocumentRewriter : IDocumentRewriter
{
    /// <summary>
    ///     Returns the original syntax tree.
    /// </summary>
    protected SyntaxTree SyntaxTree { get; private set; } = null!;

    /// <summary>
    ///     Returns the original root node.
    /// </summary>
    protected SyntaxNode RootNode { get; private set; } = null!;

    /// <inheritdoc/>
    public async Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        SyntaxTree = (await documentEditor.OriginalDocument.GetSyntaxTreeAsync())!;
        RootNode = (await documentEditor.OriginalDocument.GetSyntaxRootAsync())!;
        Run(documentEditor);
    }

    /// <summary>
    ///     Apply syntactical changes via the given document editor.
    /// </summary>
    /// <param name="documentEditor">The editor used to record the changes.</param>
    protected abstract void Run(DocumentEditor documentEditor);
}
