namespace Swallow.Refactor.Commands.Refactor.Code;

using System.ComponentModel;
using Spectre.Console.Cli;

public sealed class RefactorCodeSettings : RefactorCommandSettings
{
    [CommandOption("--filter-name")]
    [Description("Regex for the filepaths to process")]
    public string? FileName { get; set; } = null;

    [CommandOption("--filter-content")]
    [Description("Text that files need to contain to be processed")]
    public string? Content { get; set; } = null;

    [CommandArgument(position: 0, template: "<REWRITERS>")]
    [Description("Which rewriters to run; can be specified multiple times")]
    public override string[] RewriterSpecs { get; set; } = Array.Empty<string>();
}
