namespace Swallow.Refactor.Abstractions;

/// <summary>
///     A registry to resolve certain types by name.
/// </summary>
public interface IRegistry
{
    /// <summary>
    ///     Access all registered rewriters.
    /// </summary>
    IDocumentRewriterFactory DocumentRewriter { get; }

    /// <summary>
    ///     Access all registered targeted rewriters.
    /// </summary>
    ITargetedRewriterFactory TargetedRewriter { get; }

    /// <summary>
    ///     Access all registered symbol filters.
    /// </summary>
    ISymbolFilterFactory SymbolFilter { get; }
}
