namespace Swallow.Refactor.Execution.Features;

using Microsoft.CodeAnalysis;
using Settings;

/// <summary>
///     A feature loading and providing a <see cref="Workspace"/>.
/// </summary>
/// <remarks>
///     The workspace is loaded and opened before command exection iff the commands settings implement <see cref="IHasSolution"/>.
/// </remarks>
public interface IWorkspaceFeature
{
    /// <summary>
    ///     The loaded workspace.
    /// </summary>
    public Workspace Workspace { get; }
}
