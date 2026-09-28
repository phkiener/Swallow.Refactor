namespace Swallow.Refactor.Execution.Features;

using Abstractions;
using Abstractions.Filtering;
using Abstractions.Rewriting;

/// <summary>
///     A feature providing an <see cref="IRegistry"/> for <see cref="IDocumentRewriter"/>s, <see cref="ITargetedRewriter"/>s and <see cref="ISymbolFilter"/>s.
/// </summary>
public interface IRegistryFeature
{
    /// <summary>
    ///     The provided <see cref="IRegistry"/>.
    /// </summary>
    public IRegistry Registry { get; }
}
