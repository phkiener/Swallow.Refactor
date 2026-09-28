namespace Swallow.Refactor.Commands.References;

using Swallow.Refactor.Testing.Commands;

public class ListReferencesCommandTest : CommandTest<ListReferencesCommand, ListReferencesSettings>
{
    protected override ListReferencesCommand Command { get; } = new();

    [Test]
    public async Task ListsAllDirectReferencesOfProject()
    {
        var projectIds = new[] { ProjectId.CreateNewId(), ProjectId.CreateNewId() };
        var changedSolution = CurrentSolution
            .AddProject(projectId: projectIds[0], name: "First Project", assemblyName: "First", language: LanguageNames.CSharp)
            .AddProject(projectId: projectIds[1], name: "Second Project", assemblyName: "Second", language: LanguageNames.CSharp)
            .AddProjectReferences(CurrentProject.Id, new[] { new ProjectReference(projectIds[0]), new ProjectReference(projectIds[1]) });
        Workspace.TryApplyChanges(changedSolution);

        await RunCommand(new() { Project = CurrentProject.Name });

        Assert.That(TestConsole.Lines, Does.Contain("Reference: First Project"));
        Assert.That(TestConsole.Lines, Does.Contain("Reference: Second Project"));
    }

    [Test]
    public async Task ListsAllTransitiveReferencesOfProject()
    {
        var projectIds = new[] { ProjectId.CreateNewId(), ProjectId.CreateNewId() };
        var changedSolution = CurrentSolution
            .AddProject(projectId: projectIds[0], name: "First Project", assemblyName: "First", language: LanguageNames.CSharp)
            .AddProject(projectId: projectIds[1], name: "Second Project", assemblyName: "Second", language: LanguageNames.CSharp)
            .AddProjectReference(CurrentProject.Id, new(projectIds[0]))
            .AddProjectReference(projectIds[0], new(projectIds[1]));
        Workspace.TryApplyChanges(changedSolution);

        await RunCommand(new() { Project = CurrentProject.Name });

        Assert.That(TestConsole.Lines, Does.Contain("Reference: First Project"));
        Assert.That(TestConsole.Lines, Does.Contain("Transitive Reference: Second Project"));
    }
}
