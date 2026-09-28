namespace Swallow.Refactor.Commands.Interactive;

using Execution;
using Execution.Registration;
using Microsoft.CodeAnalysis.Editing;
using Spectre.Console;
using Spectre.Console.Cli;

public sealed class InteractiveSessionCommand : BaseCommand<InteractiveSessionSettings>, IRegisterableCommand
{
    protected override async Task ExecuteAsync(InteractiveSessionSettings settings)
    {
        Console.MarkupLineInterpolated($"Session opened for [blue]{Workspace.CurrentSolution.FilePath}[/].");
        Console.WriteLine("Type .quit to exit.");

        while (true)
        {
            var input = Console.Ask<string>("> ");
            var command = CommandParser.Parse(input);
            if (command is Command.Quit)
            {
                break;
            }

            if (command is Command.Help)
            {
                Console.MarkupLine("[bold]Available commands:[/]");
                Console.MarkupLine("[blue].exit[/]: Quit the interactive session");
                Console.MarkupLine("[blue].quit[/]: Quit the interactive session");
                Console.MarkupLine("[blue].help[/]: Display this help");
                Console.MarkupLine("[blue].list[/]: List all available rewriters");
                Console.MarkupLine("[blue]<rewriter> <parameters>[/]: Execute the given rewriter");

                continue;
            }

            if (command is Command.List)
            {
                Console.MarkupLine("[bold]Available rewriters:[/]");
                foreach (var rewriter in Registry.DocumentRewriter.List())
                {
                    Console.MarkupLineInterpolated($"[blue]{rewriter.Name}[/]({string.Join(", ", rewriter.Parameters.Select(p => p.Name))}): {rewriter.Description}");
                }

                continue;
            }

            if (command is Command.Describe { Name: var rewriterName })
            {
                var rewriterInfo = Registry.DocumentRewriter.List().SingleOrDefault(r => r.Name == rewriterName);
                if (rewriterInfo is null)
                {
                    Console.MarkupLineInterpolated($"Rewriter [blue]{rewriterName}[/] not found.");
                    continue;
                }
                Console.MarkupLineInterpolated($"[bold blue]{rewriterInfo.Name}[/]");
                if (rewriterInfo.Description is not null)
                {
                    Console.MarkupLineInterpolated($"{rewriterInfo.Description}");
                }

                Console.WriteLine();
                foreach (var parameter in rewriterInfo.Parameters)
                {
                    var parameterLine = $"[bold blue]{parameter.Name}[/]";
                    if (parameter.Description is not null)
                    {
                        parameterLine += $": {parameter.Description}";
                    }

                    Console.MarkupLine(parameterLine);
                }

                continue;
            }

            if (command is Command.Rewriter { Name: var name, Parameters: var parameters })
            {
                var rewriter = Registry.DocumentRewriter.Create(name, parameters);
                var solutionEditor = new SolutionEditor(Workspace.CurrentSolution);

                await Console.Progress()
                    .AutoRefresh(true)
                    .AutoClear(true)
                    .HideCompleted(true)
                    .Columns(
                        new ElapsedTimeColumn(),
                        new SpinnerColumn(),
                        new ProgressBarColumn { Width = 20 },
                        new PercentageColumn(),
                        new TaskDescriptionColumn { Alignment = Justify.Left })
                    .StartAsync(
                        async ctx =>
                        {
                            var documents = solutionEditor.OriginalSolution.Projects.SelectMany(p => p.Documents).ToList();
                            if (documents.Count is 0)
                            {
                                return;
                            }

                            var task = ctx.AddTask("Processing documents", true, documents.Count);
                            foreach (var document in documents)
                            {
                                task.Description = $"Processing {document.Name}";

                                var documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id);
                                await rewriter.RunAsync(documentEditor, documentEditor.OriginalRoot.SyntaxTree);

                                task.Increment(1);
                            }
                        });

                var changedSolution = solutionEditor.GetChangedSolution();
                Workspace.TryApplyChanges(changedSolution);

                continue;
            }

            Console.MarkupLine("[red]Unknown command.[/]");
        }
    }

    public static ICommandConfigurator RegisterWith(IConfigurator configurator)
    {
        return configurator.Register<InteractiveSessionCommand>(
            name: "interactive",
            description: "Launch an interactive session on a given solution");
    }
}
