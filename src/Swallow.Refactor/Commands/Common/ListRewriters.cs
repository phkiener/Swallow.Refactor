namespace Swallow.Refactor.Commands.Common;

using Spectre.Console;
using Execution;

internal sealed class ListRewriters : BaseCommand
{
    protected override Task ExecuteAsync(EmptySettings settings)
    {
        Console.MarkupLine("[bold]Document rewriters[/]");
        foreach (var rewriter in Registry.DocumentRewriter.List())
        {
            var rewriterLine = $"[bold blue]{rewriter.Name}[/]";
            if (rewriter.Description is not null)
            {
                rewriterLine += $": {rewriter.Description}";
            }

            Console.MarkupLine(rewriterLine);
        }

        Console.WriteLine();
        Console.MarkupLine("[bold]Targeted rewriters[/]");
        foreach (var rewriter in Registry.TargetedRewriter.List())
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
