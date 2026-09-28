namespace Swallow.Refactor.Commands.Asyncify;

using System.ComponentModel;
using Spectre.Console.Cli;
using Execution.Settings;

public sealed class AsyncifyMethodSettings : CommandSettings, IHasSolution
{
    [CommandArgument(position: 0, template: "<PROJECT>")]
    [Description("Name of the project containing the method to asyncify")]
    public string Project { get; init; } = null!;

    [CommandArgument(position: 1, template: "<METHOD>")]
    [Description("Signature of the Method to asyncify")]
    public string Method { get; init; } = null!;

    [CommandOption("-s|--solution")]
    [Description("Path to the solution to work with")]
    public string? Solution { get; init; } = null!;
}
