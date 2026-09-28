namespace Swallow.Refactor.Abstractions.Filtering;

using Microsoft.CodeAnalysis;

/// <summary>
///     A filter to determine whether to handle a certain <see cref="ISymbol"/> in processing.
/// </summary>
public interface ISymbolFilter
{
    /// <summary>
    ///     Checks whether a <see cref="ISymbol"/> should be processed further.
    /// </summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <returns><c>true</c> if the symbol should be further processed; false otherwise.</returns>
    bool Ignore(ISymbol symbol);
}
