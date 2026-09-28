namespace Swallow.Refactor.Commands.References;

using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Execution;
using Execution.Registration;
using Execution.Settings;
using static Core.Prelude;

public sealed class ListReferencesSettings : CommandSettings, IHasSolution
{
    [CommandOption("-s|--solution")]
    [Description("Path to the solution to work with")]
    public string? Solution { get; init; } = null!;

    [CommandArgument(position: 0, template: "<PROJECT>")]
    [Description("Name of the project to analyze")]
    public string Project { get; set; } = "";
}

public sealed class ListReferencesCommand : BaseCommand<ListReferencesSettings>, IRegisterableCommand
{
    protected override Task ExecuteAsync(ListReferencesSettings settings)
    {
        var rootProject = Workspace.CurrentSolution.OpenProject(settings.Project);
        var projectQueue = new Queue<ProjectId>(rootProject.ProjectReferences.Select(r => r.ProjectId));
        var allReferences = new HashSet<ProjectId>();
        while (projectQueue.TryDequeue(out var projectId))
        {
            allReferences.Add(projectId);
            foreach (var reference in Workspace.CurrentSolution.GetProject(projectId)!.ProjectReferences)
            {
                if (allReferences.Add(reference.ProjectId))
                {
                    projectQueue.Enqueue(reference.ProjectId);
                }
            }
        }

        var references = allReferences.Select(
                r => new Reference(Name: Workspace.CurrentSolution.GetProject(r)!.Name, IsTransitive: rootProject.ProjectReferences.All(pr => pr.ProjectId != r)))
            .OrderBy(r => r.IsTransitive ? 1 : 0)
            .ThenBy(r => r.Name)
            .ToList();

        foreach (var reference in references)
        {
            RenderReference(reference);
        }

        return Task.CompletedTask;
    }

    private void RenderReference(Reference reference)
    {
        if (reference.IsTransitive is false)
        {
            Console.MarkupLineInterpolated($"Reference: [blue]{reference.Name}[/]");
        }
        else
        {
            Console.MarkupLineInterpolated($"[red]Transitive Reference:[/] [blue]{reference.Name}[/]");
        }
    }

    public static ICommandConfigurator RegisterWith(IConfigurator configurator)
    {
        return configurator.Register<ListReferencesCommand>(
            name: "references",
            description: "Find all (direct and transitive) project references of a project");
    }

    private sealed record Reference(string Name, bool IsTransitive);
}
