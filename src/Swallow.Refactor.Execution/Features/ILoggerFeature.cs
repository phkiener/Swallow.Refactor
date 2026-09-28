namespace Swallow.Refactor.Execution.Features;

using Microsoft.Extensions.Logging;
using Settings;

/// <summary>
///     A feature providing an instance for an <see cref="ILogger"/>.
/// </summary>
/// <remarks>
///     This feature is always populated. If the commands settings implement <see cref="IHasLogger"/>, the logger will write to console
///     and to the file specified by <see cref="IHasLogger.Output"/>.
/// </remarks>
public interface ILoggerFeature
{
    /// <summary>
    ///     The logger instance.
    /// </summary>
    public ILogger Logger { get; }
}
