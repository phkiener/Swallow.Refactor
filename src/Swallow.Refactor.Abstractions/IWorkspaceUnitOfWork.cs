namespace Swallow.Refactor.Abstractions;

using Microsoft.CodeAnalysis;
using Rewriting;

public interface IWorkspaceUnitOfWork
{
    /// <summary>
    ///     Invoked when a <see cref="Document"/> is being processed in <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    event EventHandler<Document>? OnBeginDocument;

    /// <summary>
    ///     Invoked when a <see cref="Document"/> was completed in <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    event EventHandler<Document>? OnFinishDocument;

    /// <summary>
    ///     Invoked when a <see cref="IDocumentRewriter"/> is being processed in <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    event EventHandler<IDocumentRewriter>? OnBeginRewriter;

    /// <summary>
    ///     Invoked when a <see cref="IDocumentRewriter"/> was completed in <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    event EventHandler<IDocumentRewriter>? OnFinishRewriter;

    /// <summary>
    ///     Registers a change to be executed on <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    /// <param name="documentId">(Current) document id of the document to execute the rewriter on.</param>
    /// <param name="documentRewriter">The <see cref="IDocumentRewriter"/> to execute.</param>
    void RecordChange(DocumentId documentId, IDocumentRewriter documentRewriter);

    /// <summary>
    ///     Registers multiple changes to be executed on <see cref="IWorkspaceUnitOfWork.Execute"/>.
    /// </summary>
    /// <param name="documentId">(Current) document id of the document to execute the rewriters on.</param>
    /// <param name="rewriters">The <see cref="IDocumentRewriter"/>s to execute.</param>
    void RecordChanges(DocumentId documentId, IEnumerable<IDocumentRewriter> rewriters);

    /// <summary>
    ///     Returns the number of documents that have rewriters registered.
    /// </summary>
    int NumberOfDocuments();

    /// <summary>
    ///     Returns the number of rewriters registered for a certain document.
    /// </summary>
    /// <param name="id">Id of the document whose rewriters to count.</param>
    int NumberOfRewriters(DocumentId id);

    /// <summary>
    ///     Execute all staged changes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The changes have already been executed.</exception>
    Task Execute();
}
