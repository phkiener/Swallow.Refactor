namespace Swallow.Refactor.Abstractions.Rewriting;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

/// <summary>
///     An encapsulated change to the declaration and usaged of a <see cref="ISymbol"/>.
/// </summary>
public interface ITargetedRewriter
{
    /// <summary>
    ///     Run the rewriter for the given <paramref name="target"/> on the given <see cref="SolutionEditor"/>.
    /// </summary>
    /// <param name="solutionEditor">Editor to record the changes to.</param>
    /// <param name="target">Target symbol to act on.</param>
    Task RunAsync(SolutionEditor solutionEditor, ISymbol target);
}
