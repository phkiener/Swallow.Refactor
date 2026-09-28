namespace Swallow.Refactor.Commands.Common;

using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Execution;

internal sealed class DescribeRewriterSettings : CommandSettings
{
    [CommandArgument(position: 0, template: "<REWRITER>")]
    [Description("The rewriter to describe")]
    public string Rewriter { get; init; } = null!;
}

internal sealed class DescribeRewriter : BaseCommand<DescribeRewriterSettings>
{
    protected override Task ExecuteAsync(DescribeRewriterSettings settings)
    {
        var rewriter = Registry.DocumentRewriter.List().SingleOrDefault(r => r.Name == settings.Rewriter)
            ?? Registry.TargetedRewriter.List().SingleOrDefault(r => r.Name == settings.Rewriter);

        if (rewriter is not null)
        {
            Console.MarkupLineInterpolated($"[bold blue]{rewriter.Name}[/]");
            if (rewriter.Description is not null)
            {
                Console.MarkupLineInterpolated($"{rewriter.Description}");
            }

            Console.WriteLine();
            foreach (var parameter in rewriter.Parameters)
            {
                var parameterLine = $"[bold blue]{parameter.Name}[/]";
                if (parameter.Description is not null)
                {
                    parameterLine += $": {parameter.Description}";
                }

                Console.MarkupLine(parameterLine);
            }
        }

        return Task.CompletedTask;
    }
}
