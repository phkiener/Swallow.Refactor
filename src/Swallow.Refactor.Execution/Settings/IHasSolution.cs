namespace Swallow.Refactor.Execution.Settings;

using Features;

/// <summary>
///     Interface to denote that a command shall open a solution.
/// </summary>
public interface IHasSolution
{
    /// <summary>
    ///     Path to the solution to load - or null, if it should be found in the current directory.
    /// </summary>
    /// <remarks>
    ///     If this is <c>null</c>, the current directory is searched for a single <em>*.sln</em> file. If more than one (or none) are found,
    ///     the <see cref="IWorkspaceFeature"/> will throw an exception!
    /// </remarks>
    public string? Solution { get; }
}
