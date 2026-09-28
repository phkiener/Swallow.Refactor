namespace Swallow.Refactor.Testing.Commands;

using Features;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Execution;
using Spectre.Console;
using Swallow.Refactor.Execution.Features;
using Testing;

/// <summary>
///     Base class to test a <see cref="ProgressCommand{TSettings}"/>.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <typeparam name="TSettings">Type of the settings for the command.</typeparam>
public abstract class CommandTest<TCommand, TSettings> : RoslynTest
    where TCommand : BaseCommand<TSettings>
    where TSettings : CommandSettings
{
    /// <summary>
    ///     The <see cref="IFeatureCollection"/> that will be passed to the command.
    /// </summary>
    protected IFeatureCollection Features = null!;

    /// <summary>
    ///     The <see cref="IAnsiConsole"/> used for output of - and theoretically input to - the command.
    /// </summary>
    protected TestConsole TestConsole = null!;

    /// <summary>
    ///     The <see cref="IRegistryFeature"/> that will be used inside the test.
    /// </summary>
    protected FixedRegistry Registry = null!;

    /// <summary>
    ///     Prepare the mocked features for the command execution.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IRegistryFeature"/> is not set up by default.
    /// </remarks>
    [SetUp]
    public void PrepareFeatures()
    {
        TestConsole = new();
        Registry = new();

        Features = new TestFeatureCollection();
        Features.Set<IWorkspaceFeature>(new FixedWorkspaceFeature(Workspace));
        Features.Set<IConsoleFeature>(new FixedConsoleFeature(TestConsole));
        Features.Set<ILoggerFeature>(new ConsoleLoggerFeature(TestConsole));
        Features.Set<IRegistryFeature>(new FixedRegistryFeature(Registry));
    }

    /// <summary>
    ///     Write the written output to console after execution.
    /// </summary>
    [TearDown]
    public void WriteTestConsoleOutput()
    {
        Console.Write(TestConsole.Output);
    }

    /// <summary>
    ///     Instance of the command to test.
    /// </summary>
    protected abstract TCommand Command { get; }

    /// <summary>
    ///     Run the command, supplying the given settings and the configured features.
    /// </summary>
    /// <param name="settings">Settings for the command execution.</param>
    protected async Task RunCommand(TSettings settings)
    {
        var context = new CommandContext([], new NoRemainingArguments(), typeof(TCommand).Name, data: Features);
        await Command.ExecuteAsync(context, settings, CancellationToken.None);
    }

    private sealed class NoRemainingArguments : IRemainingArguments
    {
        public ILookup<string, string?> Parsed { get; } = Array.Empty<string>().ToLookup(s => s, s => (string?)s);
        public IReadOnlyList<string> Raw { get; } = Array.Empty<string>();
    }
}
