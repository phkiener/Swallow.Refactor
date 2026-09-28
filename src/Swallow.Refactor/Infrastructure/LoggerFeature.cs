namespace Swallow.Refactor.Infrastructure;

using Execution;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;
using Spectre.Console.Cli;
using Execution.Features;
using Execution.Settings;

public sealed class LoggerFeature : ILoggerFeature, ICommandInterceptor, IAsyncDisposable
{
    private CompositeLogger? logger;
    public ILogger Logger => (ILogger?)logger ?? NullLogger.Instance;

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Data is not IFeatureCollection featureCollection)
        {
            return;
        }

        logger = new(CreateLoggersFor(settings));
        featureCollection.Set<ILoggerFeature>(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (logger is not null)
        {
            await logger.DisposeAsync();
        }
    }

    private static IEnumerable<ILogger> CreateLoggersFor(CommandSettings settings)
    {
        yield return new AnsiConsoleLogger(ansiConsole: AnsiConsole.Console).EnableFor(LogLevel.Trace, LogLevel.Warning, LogLevel.Error);

        if (settings is IHasLogger { Output: not null } loggerSettings)
        {
            yield return new FileLogger(path: loggerSettings.Output).EnableFor(LogLevel.Information);
        }
    }
}
