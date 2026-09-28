namespace Swallow.Refactor.Testing.Commands.Features;

using Execution.Features;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Testing;

/// <summary>
///     A feature that will provide a logger that will only log to an <see cref="IAnsiConsole"/> - typically, a <see cref="TestConsole"/>.
/// </summary>
public sealed class ConsoleLoggerFeature : ILoggerFeature
{
    public ConsoleLoggerFeature(IAnsiConsole console)
    {
        Logger = new ConsoleLogger(console);
    }

    /// <inheritdoc/>
    public ILogger Logger { get; }

    private sealed class ConsoleLogger : ILogger
    {
        private readonly IAnsiConsole ansiConsole;

        public ConsoleLogger(IAnsiConsole ansiConsole)
        {
            this.ansiConsole = ansiConsole;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ansiConsole.WriteLine($"[{logLevel}:{eventId}] {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
    }
}
