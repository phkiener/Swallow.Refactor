namespace Swallow.Refactor.Infrastructure.Logging;

using Microsoft.Extensions.Logging;
using Spectre.Console;

internal sealed class AnsiConsoleLogger : BasicLoggerBase
{
    private readonly IAnsiConsole ansiConsole;

    public AnsiConsoleLogger(IAnsiConsole ansiConsole)
    {
        this.ansiConsole = ansiConsole;
    }

    protected override void WriteMessage(LogLevel logLevel, string message)
    {
        var escapedMessage = Markup.Escape(message);
        var markupLine = logLevel switch
        {
            LogLevel.Trace => $"[grey][[TRCE]] {escapedMessage}[/]",
            LogLevel.Debug => $"[grey][[DEBG]] {escapedMessage}[/]",
            LogLevel.Information => $"[white][[INFO]] {escapedMessage}[/]",
            LogLevel.Warning => $"[orange][[WARN]] {escapedMessage}[/]",
            LogLevel.Error => $"[red][[ERRO]] {escapedMessage}[/]",
            LogLevel.Critical => $"[red][[CRIT]] {escapedMessage}[/]",
            LogLevel.None => escapedMessage,
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(logLevel), actualValue: logLevel, message: null)
        };

        ansiConsole.MarkupLine(markupLine);
    }
}
