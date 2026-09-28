namespace Swallow.Refactor.Abstractions.Filtering;

using Microsoft.CodeAnalysis;

/// <summary>
///     A document filter to fine-tune which documents to process.
/// </summary>
public interface IDocumentFilter
{
    /// <summary>
    ///     Checks if the given document matches the criteria.
    /// </summary>
    /// <param name="document">Document to check.</param>
    /// <returns><c>true</c> if the document matches the criteria, <c>false</c> otherwise.</returns>
    Task<bool> Matches(Document document);
}
