namespace Swallow.Refactor.Infrastructure;

using System.Diagnostics;
using Core;
using Execution;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Execution.Features;
using Execution.Settings;

public sealed class WorkspaceFeature : IWorkspaceFeature, ICommandInterceptor
{
    private Workspace? loadedWorkspace;
    public Workspace Workspace => loadedWorkspace ?? throw new InvalidOperationException("No workspace has been loaded.");

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Data is not IFeatureCollection featureCollection || settings is not IHasSolution hasSolution)
        {
            return;
        }

        var solutionPath = FindSolution(hasSolution);
        var logger = featureCollection.Get<ILoggerFeature>()?.Logger;
        logger?.LogTrace("Opening solution {Path}...", solutionPath);

        var stopwatch = Stopwatch.StartNew();
        var workspace = Prelude.OpenWorkspace(solutionPath).GetAwaiter().GetResult();
        stopwatch.Stop();

        logger?.LogTrace("Opened solution in {Time:g}.", stopwatch.Elapsed);

        loadedWorkspace = workspace;
        featureCollection.Set<IWorkspaceFeature>(this);
    }

    private static string FindSolution(IHasSolution hasSolution)
    {
        if (hasSolution.Solution is not null)
        {
            return hasSolution.Solution;
        }

        return Directory.GetFiles(path: Environment.CurrentDirectory, searchPattern: ".sln", searchOption: SearchOption.TopDirectoryOnly) switch
        {
            { Length: 0 } => throw new InvalidOperationException($"No .sln file found in {Environment.CurrentDirectory}."),
            { Length: > 1 } => throw new InvalidOperationException($"Found multiple .sln files in {Environment.CurrentDirectory}."),
            [var file] => file
        };
    }
}
