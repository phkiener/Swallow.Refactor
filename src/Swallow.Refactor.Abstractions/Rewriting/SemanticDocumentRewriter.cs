namespace Swallow.Refactor.Abstractions.Rewriting;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

/// <summary>
///     A base rewriter where the semantic model for the document has already been computed.
/// </summary>
/// <remarks>
///     If you do not require semantic analysis, better derive from <see cref="SyntaxDocumentRewriter"/>. It's way faster!
/// </remarks>
public abstract class SemanticDocumentRewriter : IDocumentRewriter
{
    /// <summary>
    ///     Returns the original syntax tree.
    /// </summary>
    protected SyntaxTree SyntaxTree { get; private set; } = null!;

    /// <summary>
    ///     Returns the original root node.
    /// </summary>
    protected SyntaxNode RootNode { get; private set; } = null!;

    /// <summary>
    ///     The semantic model for the original document.
    /// </summary>
    protected SemanticModel SemanticModel { get; private set; } = null!;

    /// <inheritdoc/>
    public async Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        SyntaxTree = (await documentEditor.OriginalDocument.GetSyntaxTreeAsync())!;
        RootNode = (await documentEditor.OriginalDocument.GetSyntaxRootAsync())!;
        SemanticModel = (await documentEditor.OriginalDocument.GetSemanticModelAsync())!;
        Run(documentEditor);
    }

    /// <summary>
    ///     Apply syntactical changes via the given document editor.
    /// </summary>
    /// <param name="documentEditor">The editor used to record the changes.</param>
    protected abstract void Run(DocumentEditor documentEditor);
}
