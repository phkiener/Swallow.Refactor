namespace Swallow.Refactor.Infrastructure;

using Execution;
using Execution.Features;
using Spectre.Console;
using Spectre.Console.Cli;

public sealed class AnsiConsoleFeature : IConsoleFeature, ICommandInterceptor
{
    public IAnsiConsole Console { get; } = AnsiConsole.Console;

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Data is not IFeatureCollection featureCollection)
        {
            return;
        }

        featureCollection.Set<IConsoleFeature>(this);
    }
}
