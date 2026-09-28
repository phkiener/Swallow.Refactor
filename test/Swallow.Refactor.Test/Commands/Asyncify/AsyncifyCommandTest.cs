namespace Swallow.Refactor.Commands.Asyncify;

using Testing.Commands;

internal sealed partial class AsyncifyCommandTest : CommandTest<AsyncifyMethodCommand, AsyncifyMethodSettings>
{
    protected override AsyncifyMethodCommand Command { get; } = new();

    private async Task RunAsyncifier(string methodDeclaration)
    {
        await RunCommand(new() { Method = methodDeclaration, Project = CurrentProject.Name });
    }
}
