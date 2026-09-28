namespace Swallow.Refactor.Commands.Refactor.Symbol;

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

public sealed class RefactorSymbolSettings : RefactorCommandSettings
{
    [CommandOption("--type")]
    [Description("Name of the type to refactor")]
    public string? Type { get; set; } = null;

    [CommandOption("--member")]
    [Description("Name of the member to refactor")]
    public string? Member { get; set; } = null;

    [CommandOption("--cursor")]
    [Description("Cursor which is on the symbol to refactor, in the form 'filename;line:column', e.g. 'Program.cs;15:4")]
    public string? Cursor { get; set; } = null!;

    [CommandArgument(position: 0, template: "<REWRITERS>")]
    [Description("Which rewriters to run; can be specified multiple times")]
    public override string[] RewriterSpecs { get; set; } = Array.Empty<string>();

    public override ValidationResult Validate()
    {
        return Type is null && Cursor is null
            ? ValidationResult.Error($"Either a {nameof(Cursor)} or a {nameof(Type)} (plus optionally a {nameof(Member)} must be specified.")
            : ValidationResult.Success();
    }
}
