namespace Swallow.Refactor.Core.SymbolFilters;

using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

/// <summary>
///     A symbol filter to exclude controller actions, since they're called via reflection.
/// </summary>
public sealed class IsControllerAction : ISymbolFilter
{
    private const string ControllerBaseTypeName = "ControllerBase";
    private const string ActionResultTypeName = "IActionResult";
    private const string AsyncResultTypeName = "IAsyncResult";

    /// <inheritdoc />
    public bool Ignore(ISymbol symbol)
    {
        var isInController = IsController(symbol.ContainingType);
        var returnsActionResult = symbol is IMethodSymbol ms && (IsActionResult(ms.ReturnType) || IsAsyncActionResult(ms.ReturnType));

        return isInController && returnsActionResult;
    }

    private static bool IsController(INamedTypeSymbol symbolContainingType)
    {
        var currentClass = symbolContainingType;
        while (currentClass is not null)
        {
            if (currentClass.Name == ControllerBaseTypeName)
            {
                return true;
            }

            currentClass = currentClass.BaseType;
        }

        return false;
    }

    private static bool IsActionResult(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Name == ActionResultTypeName || typeSymbol.AllInterfaces.Any(i => i.Name == ActionResultTypeName);
    }

    private static bool IsAsyncActionResult(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            var isTask = IsTask(namedTypeSymbol) && namedTypeSymbol.TypeArguments.Length == 1;

            return isTask && IsActionResult(namedTypeSymbol.TypeArguments.Single());
        }

        return false;
    }

    private static bool IsTask(ITypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.Name == AsyncResultTypeName || namedTypeSymbol.AllInterfaces.Any(i => i.Name == AsyncResultTypeName);
    }
}
