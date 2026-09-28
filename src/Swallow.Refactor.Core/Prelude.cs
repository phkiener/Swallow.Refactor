namespace Swallow.Refactor.Core;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

/// <summary>
///     The <em>Prelude</em> contains common helper methods beneficial for each scenario.
/// </summary>
public static class Prelude
{
    private static readonly Dictionary<string, string> workspaceProperties = new() { ["AnalyzerState"] = "off" };

    /// <summary>
    ///     Create a <see cref="Workspace"/> containing the <see cref="Solution"/> at the given <paramref name="solutionPath"/>.
    /// </summary>
    /// <param name="solutionPath">Path to the <c>.sln</c> file to open.</param>
    /// <returns>A workspace where the given solution is loaded.</returns>
    public static async Task<Workspace> OpenWorkspace(string solutionPath)
    {
        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create(workspaceProperties);
        await workspace.OpenSolutionAsync(solutionPath);

        return workspace;
    }

    /// <summary>
    ///     Open the <see cref="Solution"/> at the given path.
    /// </summary>
    /// <param name="solutionPath">Path to the <c>.sln</c> file to open.</param>
    /// <returns>The requested solution.</returns>
    public static async Task<Solution> OpenSolution(string solutionPath)
    {
        var workspace = await OpenWorkspace(solutionPath);

        return workspace.CurrentSolution;
    }

    /// <summary>
    ///     Get a <see cref="Project"/> from the solution.
    /// </summary>
    /// <param name="solution">The solution containing the project.</param>
    /// <param name="projectName">Name of the project to fetch.</param>
    /// <returns>The found project.</returns>
    /// <exception cref="KeyNotFoundException">The project is not found in the solution.</exception>
    public static Project OpenProject(this Solution solution, string projectName)
    {
        return solution.Projects.SingleOrDefault(p => p.Name == projectName)
               ?? throw new KeyNotFoundException($"Project {projectName} not found in solution.");
    }

    /// <summary>
    ///     A symbol display format containing a minmal signature (e.g. for logging).
    /// </summary>
    public static SymbolDisplayFormat ShortSymbolDisplayFormat
        => SymbolDisplayFormat.MinimallyQualifiedFormat.RemoveMemberOptions(SymbolDisplayMemberOptions.IncludeType); // Without return type
}
