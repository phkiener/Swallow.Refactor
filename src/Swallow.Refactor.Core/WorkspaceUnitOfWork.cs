namespace Swallow.Refactor.Core;

using System.Collections.Immutable;
using Abstractions;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

/// <summary>
///     A unit of work facilitating changes to multiple documents in a solution.
/// </summary>
public sealed class WorkspaceUnitOfWork : IDisposable, IWorkspaceUnitOfWork
{
    private readonly Workspace workspace;
    private readonly IDictionary<DocumentId, ImmutableList<IDocumentRewriter>> rewritersByDocument;
    private bool changesCommitted;

    /// <inheritdoc/>
    public event EventHandler<Document>? OnBeginDocument;

    /// <inheritdoc/>
    public event EventHandler<Document>? OnFinishDocument;

    /// <inheritdoc/>
    public event EventHandler<IDocumentRewriter>? OnBeginRewriter;

    /// <inheritdoc/>
    public event EventHandler<IDocumentRewriter>? OnFinishRewriter;

    internal WorkspaceUnitOfWork(Workspace workspace)
    {
        this.workspace = workspace;
        rewritersByDocument = new Dictionary<DocumentId, ImmutableList<IDocumentRewriter>>();
    }

    /// <inheritdoc/>
    public void RecordChange(DocumentId documentId, IDocumentRewriter documentRewriter)
    {
        if (rewritersByDocument.ContainsKey(documentId))
        {
            rewritersByDocument[documentId] = rewritersByDocument[documentId].Add(documentRewriter);
        }
        else
        {
            rewritersByDocument[documentId] = ImmutableList.Create(documentRewriter);
        }
    }

    /// <inheritdoc/>
    public void RecordChanges(DocumentId documentId, IEnumerable<IDocumentRewriter> rewriters)
    {
        foreach (var rewriter in rewriters)
        {
            RecordChange(documentId: documentId, documentRewriter: rewriter);
        }
    }

    /// <inheritdoc/>
    public int NumberOfDocuments()
    {
        return rewritersByDocument.Keys.Count;
    }

    /// <inheritdoc/>
    public int NumberOfRewriters(DocumentId id)
    {
        return rewritersByDocument[id].Count;
    }

    /// <inheritdoc/>
    public async Task Execute()
    {
        if (changesCommitted)
        {
            throw new InvalidOperationException("Changes have already been executed.");
        }

        foreach (var (id, rewriters) in rewritersByDocument)
        {
            var document = workspace.CurrentSolution.GetDocument(id);
            if (document is null || rewriters.Count == 0)
            {
                continue;
            }

            OnBeginDocument?.Invoke(sender: this, e: document);
            foreach (var rewriter in rewriters)
            {
                OnBeginRewriter?.Invoke(sender: this, e: rewriter);
                document = await RunRewriter(documentRewriter: rewriter, document: document);
                OnFinishRewriter?.Invoke(sender: this, e: rewriter);
            }

            await ApplyChanges(document);
            OnFinishDocument?.Invoke(sender: this, e: document);
        }

        changesCommitted = true;
    }

    private static async Task<Document> RunRewriter(IDocumentRewriter documentRewriter, Document document)
    {
        var editor = await DocumentEditor.CreateAsync(document);
        await documentRewriter.RunAsync(documentEditor: editor, syntaxTree: editor.OriginalRoot.SyntaxTree);

        return editor.GetChangedDocument();
    }

    private async Task ApplyChanges(Document changedDocument)
    {
        var modifiedSyntax = await changedDocument.GetSyntaxRootAsync();
        var changedSolution = workspace.CurrentSolution.WithDocumentSyntaxRoot(documentId: changedDocument.Id, root: modifiedSyntax!);
        workspace.TryApplyChanges(changedSolution);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() { }
}

public static class WorkspaceUnitOfWorkExtensions
{
    /// <summary>
    ///     Create a new <see cref="WorkspaceUnitOfWork"/> for the given workspace.
    /// </summary>
    /// <param name="workspace">The workspace to commit the changes to.</param>
    /// <returns>A unit of work for modification and committing the changes.</returns>
    public static WorkspaceUnitOfWork BeginChanges(this Workspace workspace)
    {
        return new(workspace);
    }
}
