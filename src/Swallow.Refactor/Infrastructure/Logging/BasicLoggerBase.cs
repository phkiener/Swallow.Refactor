namespace Swallow.Refactor.Infrastructure.Logging;

using Microsoft.Extensions.Logging;

internal abstract class BasicLoggerBase : ILogger
{
    private readonly ISet<LogLevel> enabledFor = new HashSet<LogLevel>();

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(arg1: state, arg2: exception);
        WriteMessage(logLevel: logLevel, message: message);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return enabledFor.Contains(logLevel);
    }

    public BasicLoggerBase EnableFor(LogLevel logLevel)
    {
        enabledFor.Add(logLevel);

        return this;
    }

    public BasicLoggerBase EnableFor(params LogLevel[] logLevel)
    {
        return logLevel.Aggregate(seed: this, func: (logger, level) => logger.EnableFor(level));
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException("Don't use Scopes, I don't care to implement them.");
    }

    protected abstract void WriteMessage(LogLevel logLevel, string message);
}
