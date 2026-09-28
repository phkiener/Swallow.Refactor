namespace Swallow.Refactor.Abstractions;

using Rewriting;

public interface ITargetedRewriterFactory
{
    /// <summary>
    ///     Create the <see cref="ITargetedRewriter"/> with the given name and parameters.
    /// </summary>
    /// <param name="name">Name of the rewriter to create.</param>
    /// <param name="parameters">Parameters for the rewriter, in the expected order.</param>
    /// <returns>The created rewriter.</returns>
    ITargetedRewriter Create(string name, params string[] parameters);

    /// <summary>
    ///     List all registered <see cref="ITargetedRewriter"/>s.
    /// </summary>
    IReadOnlyCollection<IRewriterInfo> List();
}
