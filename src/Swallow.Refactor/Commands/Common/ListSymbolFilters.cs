namespace Swallow.Refactor.Commands.Common;

using Spectre.Console;
using Execution;

internal sealed class ListSymbolFilters : BaseCommand
{
    protected override Task ExecuteAsync(EmptySettings settings)
    {
        foreach (var rewriter in Registry.SymbolFilter.List())
        {
            var rewriterLine = $"[bold blue]{rewriter.Name}[/]";
            if (rewriter.Description is not null)
            {
                rewriterLine += $": {rewriter.Description}";
            }

            Console.MarkupLine(rewriterLine);
        }

        return Task.CompletedTask;
    }
}
