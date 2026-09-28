namespace Swallow.Refactor.Abstractions.Rewriting;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

/// <summary>
///     A parameterized (i.e. configurable) version of a document modification.
/// </summary>
public interface IDocumentRewriter
{
    /// <summary>
    ///     Run the rewriter on the given <see cref="DocumentEditor"/>.
    /// </summary>
    /// <param name="documentEditor">Editor on the document to modify.</param>
    /// <param name="syntaxTree">The syntax tree contained in the document.</param>
    Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree);
}
