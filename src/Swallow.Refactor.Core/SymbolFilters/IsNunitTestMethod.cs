namespace Swallow.Refactor.Core.SymbolFilters;

using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

/// <summary>
///     A symbol filter to exclude methods used in test classes.
/// </summary>
/// <remarks>
///     This includes test method, setup methods and teardown methods.
/// </remarks>
public sealed class IsNunitTestMethod : ISymbolFilter
{
    /// <inheritdoc />
    public bool Ignore(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(IsNunitAttribute);
    }

    private static bool IsNunitAttribute(AttributeData attribute)
    {
        return attribute.AttributeClass?.ContainingNamespace.ToString() == "NUnit.Framework";
    }
}
