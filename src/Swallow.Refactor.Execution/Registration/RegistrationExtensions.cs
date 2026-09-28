namespace Swallow.Refactor.Execution.Registration;

using Spectre.Console.Cli;

public static class RegistrationExtensions
{
    /// <summary>
    ///     Register the given command without settings.
    /// </summary>
    /// <param name="configurator">The configurator to use.</param>
    /// <param name="name">Name for the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="examples">A list of examples, each specified by a list of arguments.</param>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <returns>The finished configurator.</returns>
    public static ICommandConfigurator Register<TCommand>(
        this IConfigurator configurator,
        string name,
        string description,
        params string[][] examples) where TCommand : class, ICommand
    {
        var command = configurator.AddCommand<TCommand>(name).WithDescription(description);
        foreach (var example in examples)
        {
            command = command.WithExample(example);
        }

        return command;
    }

    /// <summary>
    ///     Register the given command with settings.
    /// </summary>
    /// <param name="configurator">The configurator to use.</param>
    /// <param name="name">Name for the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="examples">A list of examples, each specified by a list of arguments.</param>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <returns>The finished configurator.</returns>
    public static ICommandConfigurator Register<TCommand>(
        this IConfigurator<CommandSettings> configurator,
        string name,
        string description,
        params string[][] examples) where TCommand : class, ICommandLimiter<CommandSettings>
    {
        var command = configurator.AddCommand<TCommand>(name).WithDescription(description);
        foreach (var example in examples)
        {
            command = command.WithExample(example);
        }

        return command;
    }

    /// <summary>
    ///     Register the given command with settings.
    /// </summary>
    /// <param name="configurator">The configurator to use.</param>
    /// <param name="name">Name for the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="examples">A list of examples, each specified by a list of arguments.</param>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The type of settings the command accepts.</typeparam>
    /// <returns>The finished configurator.</returns>
    public static ICommandConfigurator Register<TCommand, TSettings>(
        this IConfigurator<TSettings> configurator,
        string name,
        string description,
        params string[][] examples)
        where TCommand : class, ICommandLimiter<TSettings>
        where TSettings : CommandSettings
    {
        var command = configurator.AddCommand<TCommand>(name).WithDescription(description);
        foreach (var example in examples)
        {
            command = command.WithExample(example);
        }

        return command;
    }

    /// <summary>
    ///     Register a branch to gather multiple commands with the same prefix.
    /// </summary>
    /// <param name="configurator">The configurator to use.</param>
    /// <param name="name">Name for the branch.</param>
    /// <param name="description">Description of the branch.</param>
    /// <param name="commandRegistrations">Function registering all commands and returning their respective configurators.</param>
    /// <typeparam name="TSettings">Type of the shared settings.</typeparam>
    /// <returns>The finished configurator.</returns>
    /// <remarks>
    ///     The returned <see cref="ICommandConfigurator"/> is a bit special;
    ///     it only supports <see cref="ICommandConfigurator.IsHidden"/> and <see cref="ICommandConfigurator.WithData"/>.
    ///     Everything else will throw an exception
    /// </remarks>
    public static ICommandConfigurator RegisterBranch<TSettings>(
        this IConfigurator configurator,
        string name,
        string description,
        Func<IConfigurator<TSettings>, IEnumerable<ICommandConfigurator>> commandRegistrations)
        where TSettings : CommandSettings
    {
        var commandsInBranch = new List<ICommandConfigurator>();
        configurator.AddBranch<TSettings>(name,
            branchConfigurator =>
            {
                branchConfigurator.SetDescription(description);

                var subcommands = commandRegistrations(branchConfigurator);
                commandsInBranch.AddRange(subcommands);
            });

        return new MultiCommandConfigurator(commandsInBranch);
    }

    private sealed class MultiCommandConfigurator(IReadOnlyCollection<ICommandConfigurator> configurators) : ICommandConfigurator
    {
        public ICommandConfigurator WithExample(params string[] args)
        {
            throw OperationNotSupportedException();
        }

        public ICommandConfigurator WithAlias(string name)
        {
            throw OperationNotSupportedException();
        }

        public ICommandConfigurator WithDescription(string description)
        {
            throw OperationNotSupportedException();
        }

        public ICommandConfigurator WithData(object data)
        {
            foreach (var configurator in configurators)
            {
                configurator.WithData(data);
            }

            return this;
        }

        public ICommandConfigurator IsHidden()
        {
            foreach (var c in configurators)
            {
                c.IsHidden();
            }

            return this;
        }

        private static NotSupportedException OperationNotSupportedException()
        {
            return new($"The configurator retrieved by calling {nameof(RegisterBranch)} supports only {nameof(WithData)} and {nameof(IsHidden)}");
        }
    }
}
