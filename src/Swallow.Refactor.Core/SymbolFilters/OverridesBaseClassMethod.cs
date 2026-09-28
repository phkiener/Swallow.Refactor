namespace Swallow.Refactor.Core.SymbolFilters;

using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

/// <summary>
///     A symbol filter to exclude all overrides.
/// </summary>
/// <remarks>
///     Either the base class is defined outside, which means we couldn't get rid of the method, or it will be checked as well - no need to double check.
/// </remarks>
public sealed class OverridesBaseClassMethod : ISymbolFilter
{
    /// <inheritdoc />
    public bool Ignore(ISymbol symbol)
    {
        return symbol.IsOverride;
    }
}
