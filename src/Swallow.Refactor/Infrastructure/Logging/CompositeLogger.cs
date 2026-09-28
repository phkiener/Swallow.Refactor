namespace Swallow.Refactor.Infrastructure.Logging;

using Microsoft.Extensions.Logging;

internal sealed class CompositeLogger : ILogger, IAsyncDisposable
{
    private readonly IReadOnlyCollection<ILogger> loggers;

    public CompositeLogger(IEnumerable<ILogger> loggers)
    {
        this.loggers = loggers.ToList();
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        foreach (var logger in loggers.Where(l => l.IsEnabled(logLevel)))
        {
            logger.Log(
                logLevel: logLevel,
                eventId: eventId,
                state: state,
                exception: exception,
                formatter: formatter);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException("Don't use Scopes, I don't care to implement them.");
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in loggers.OfType<IAsyncDisposable>())
        {
            await disposable.DisposeAsync();
        }
    }
}
