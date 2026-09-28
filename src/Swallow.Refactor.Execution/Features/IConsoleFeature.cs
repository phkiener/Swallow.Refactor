namespace Swallow.Refactor.Execution.Features;

using Spectre.Console;

/// <summary>
///     A feature providng an <see cref="IAnsiConsole"/>.
/// </summary>
public interface IConsoleFeature
{
    /// <summary>
    ///     The provided console instance.
    /// </summary>
    public IAnsiConsole Console { get; }
}
