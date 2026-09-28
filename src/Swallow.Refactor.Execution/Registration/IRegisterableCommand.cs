namespace Swallow.Refactor.Execution.Registration;

using Spectre.Console.Cli;

/// <summary>
///     Interface for a command that can be registered and will be picked up automatically.
/// </summary>
public interface IRegisterableCommand
{
    /// <summary>
    ///     Register the command in the CLI.
    /// </summary>
    /// <param name="configurator">The configurator to use.</param>
    /// <returns>The given configurator.</returns>
    /// <seealso cref="RegistrationExtensions"/>
    static abstract ICommandConfigurator RegisterWith(IConfigurator configurator);
}
