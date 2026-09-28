namespace Swallow.Refactor.Commands.Unused;

using System.ComponentModel;
using Spectre.Console.Cli;
using Execution.Settings;

public sealed class FindUnusedSettings : CommandSettings, IHasLogger, IHasSolution
{
    [CommandOption("-s|--solution")]
    [Description("Path to the solution to work with")]
    public string? Solution { get; init; } = null!;

    [CommandOption("-o|--output")]
    [Description("Where to write the generated usage-log to")]
    public string Output { get; set; } = null!;

    [CommandArgument(position: 0, template: "<PROJECT>")]
    [Description("Name of the project to analyze")]
    public string Project { get; set; } = "";
}
