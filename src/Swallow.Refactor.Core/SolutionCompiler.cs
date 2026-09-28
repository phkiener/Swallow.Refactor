namespace Swallow.Refactor.Core;

using Microsoft.CodeAnalysis;

/// <summary>
/// A utility helper to pre-compile a whole solution.
/// </summary>
public class SolutionCompiler
{
    /// <summary>
    /// Iterate through all projects, compiling them one-by-one.
    /// </summary>
    /// <param name="solution">The <see cref="Solution"/> to compile.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel compilation.</param>
    /// <returns>A task that will complete when <em>all</em> projects have been compiled.</returns>
    public static async Task CompileAsync(Solution solution, CancellationToken cancellationToken = default)
    {
        var orderedProjects = solution.GetProjectDependencyGraph().GetTopologicallySortedProjects(cancellationToken).ToList();
        foreach (var projectId in orderedProjects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var project = solution.GetProject(projectId);
            if (project is not null)
            {
                await project.GetCompilationAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Compile all projects in parallel, using the supplied <see cref="TaskScheduler"/>.
    /// </summary>
    /// <param name="solution">The <see cref="Solution"/> to compile.</param>
    /// <param name="taskScheduler">A <see cref="TaskScheduler"/> to control execution of tasks; defaults to <see cref="TaskScheduler.Current"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel compilation.</param>
    /// <returns>A task that will complete when <em>all</em> projects have been compiled.</returns>
    public static async Task CompileParallelAsync(Solution solution, TaskScheduler? taskScheduler = null, CancellationToken cancellationToken = default)
    {
        taskScheduler ??= TaskScheduler.Current;
        var taskFactory = new TaskFactory(taskScheduler);

        var orderedProjects = solution.GetProjectDependencyGraph().GetTopologicallySortedProjects(cancellationToken).ToList();
        var compilationStateByProjectId = orderedProjects.ToDictionary(static p => p, static _ => WorkingState.NotStarted);
        var dependenciesByProjectId = orderedProjects.ToDictionary(
            static p => p,
            p => solution.GetProject(p)?.ProjectReferences.Select(r => r.ProjectId).ToHashSet() ?? []);

        var compilationTasks = new HashSet<Task>();
        while (compilationStateByProjectId.Any(c => c.Value is not WorkingState.Completed))
        {
            foreach (var projectId in orderedProjects)
            {
                if (solution.GetProject(projectId) is not { } project || compilationStateByProjectId[projectId] is not WorkingState.NotStarted)
                {
                    continue;
                }

                if (dependenciesByProjectId[projectId].All(d => compilationStateByProjectId[d] is WorkingState.Completed))
                {
                    compilationStateByProjectId[project.Id] = WorkingState.Working;

                    var task = taskFactory.FromAsync(project.GetCompilationAsync(cancellationToken), _ => compilationStateByProjectId[project.Id] = WorkingState.Completed);
                    compilationTasks.Add(task);
                }
            }

            var completedTask = await Task.WhenAny(compilationTasks);
            compilationTasks.Remove(completedTask);
        }
    }

    private enum WorkingState { NotStarted, Working, Completed }
}
