namespace Swallow.Refactor.Commands.Interactive;

using System.ComponentModel;
using Execution.Settings;
using Spectre.Console.Cli;

public sealed class InteractiveSessionSettings : CommandSettings, IHasSolution
{
    [CommandOption("-s|--solution")]
    [Description("Path to the solution to work with")]
    public string? Solution { get; init; } = null!;
}
