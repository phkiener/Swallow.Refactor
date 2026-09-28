namespace Swallow.Refactor.Execution.Settings;

using Microsoft.Extensions.Logging;

/// <summary>
///     Interface to denote that a command shall log its output to a file.
/// </summary>
/// <remarks>
///     Only <see cref="LogLevel.Information"/> level logs are printed to <see cref="Output"/>.
/// </remarks>
public interface IHasLogger
{
    /// <summary>
    ///     Path to the file where the log shall written to.
    /// </summary>
    public string? Output { get; }
}
