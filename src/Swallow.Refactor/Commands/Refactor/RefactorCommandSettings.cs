namespace Swallow.Refactor.Commands.Refactor;

using System.ComponentModel;
using Spectre.Console.Cli;
using Execution.Settings;

public abstract class RefactorCommandSettings : CommandSettings, IHasSolution
{
    [CommandOption("-s|--solution")]
    [Description("Path to the solution to work with")]
    public string? Solution { get; init; } = null!;

    // RewriterSpecs is abstract here to make sure the help text does not include <REWRITERS> before the actual command to execute.
    public abstract string[] RewriterSpecs { get; set; }
}
