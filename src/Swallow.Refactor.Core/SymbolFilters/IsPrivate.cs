namespace Swallow.Refactor.Core.SymbolFilters;

using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

/// <summary>
///     A symbol filter to exclude all symbols that are private, since the IDE is smart enough to detect unused private members.
/// </summary>
public sealed class IsPrivate : ISymbolFilter
{
    /// <inheritdoc />
    public bool Ignore(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Private;
    }
}
