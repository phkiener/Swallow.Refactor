namespace Swallow.Refactor.Execution;

using Spectre.Console;
using Spectre.Console.Cli;

/// <summary>
///     Helper override for a progress command without settings.
/// </summary>
public abstract class ProgressCommand : ProgressCommand<BaseCommand.EmptySettings>;

/// <summary>
///     Base class for a command to be executed via CLI.
/// </summary>
/// <remarks>
///     In contrast to a <see cref="BaseCommand"/>, the <see cref="ProgressCommand"/> wraps the execution inside a
///     <see cref="AnsiConsole.Progress"/> which allows for progress bars to be displayed.
///     On the other hand, it removes the ability to use any of <see cref="AnsiConsole"/>'s live rendering methods.
/// </remarks>
/// <typeparam name="TSettings">Type of the settings object for the command.</typeparam>
public abstract partial class ProgressCommand<TSettings> : BaseCommand<TSettings> where TSettings : CommandSettings
{
    private ProgressContext progessContext = null!;

    /// <inheritdoc />
    protected sealed override async Task ExecuteAsync(TSettings settings)
    {
        await Console.Progress()
            .HideCompleted(true)
            .AutoClear(true)
            .Columns(
                new ElapsedTimeColumn(),
                new SpinnerColumn(),
                new ProgressBarColumn { Width = 20 },
                new PercentageColumn(),
                new TaskDescriptionColumn { Alignment = Justify.Left })
            .StartAsync(
                async c =>
                {
                    progessContext = c;
                    await RunAsync(settings);
                });
    }

    /// <summary>
    ///     Execute the command with the given settings.
    /// </summary>
    /// <param name="settings">Settings specified for the execution.</param>
    protected abstract Task RunAsync(TSettings settings);
}
