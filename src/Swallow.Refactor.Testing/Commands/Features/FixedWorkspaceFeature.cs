namespace Swallow.Refactor.Testing.Commands.Features;

using Execution.Features;

/// <summary>
///     A feature that will provide a fixed, given workspace - typically, an <see cref="AdhocWorkspace"/>.
/// </summary>
public sealed class FixedWorkspaceFeature : IWorkspaceFeature
{
    public FixedWorkspaceFeature(Workspace workspace)
    {
        Workspace = workspace;
    }

    /// <inheritdoc/>
    public Workspace Workspace { get; }
}
