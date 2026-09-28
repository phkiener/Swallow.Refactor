namespace Swallow.Refactor.Testing.Commands.Features;

using Execution.Features;
using Spectre.Console;
using Spectre.Console.Testing;

/// <summary>
///     A feature that will provide a fixed, given console - typically, a <see cref="TestConsole"/>.
/// </summary>
public sealed class FixedConsoleFeature : IConsoleFeature
{
    public FixedConsoleFeature(IAnsiConsole console)
    {
        Console = console;
    }

    /// <inheritdoc/>
    public IAnsiConsole Console { get; }
}
