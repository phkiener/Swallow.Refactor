namespace Swallow.Refactor.Execution;

using Abstractions;
using Features;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Settings;
using Spectre.Console;
using Spectre.Console.Cli;

/// <summary>
///     Helper override for a command without settings.
/// </summary>
public abstract class BaseCommand : BaseCommand<BaseCommand.EmptySettings>
{
    public sealed class EmptySettings : CommandSettings;
}

/// <summary>
///     Base class for a command to be executed via CLI.
/// </summary>
/// <typeparam name="TSettings">Type of the settings object for the command.</typeparam>
public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings> where TSettings : CommandSettings
{
    /// <summary>
    ///     The collection of registered features.
    /// </summary>
    protected IFeatureCollection FeatureCollection { get; private set; } = null!;

    /// <summary>
    ///     The preloaded solution if <typeparamref name="TSettings"/> implements <see cref="IHasSolution"/> or a new, <see cref="AdhocWorkspace"/>.
    /// </summary>
    protected Workspace Workspace => FeatureCollection.Get<IWorkspaceFeature>()?.Workspace ?? new AdhocWorkspace();

    /// <summary>
    ///     The configured <see cref="ILogger"/> for this command.
    /// </summary>
    protected ILogger Logger => FeatureCollection.Get<ILoggerFeature>()?.Logger ?? NullLogger.Instance;

    /// <summary>
    ///     The <see cref="IRegistry"/> build from registered assemblies.
    /// </summary>
    protected IRegistry Registry => FeatureCollection.Get<IRegistryFeature>()?.Registry ?? new NullRegistry();

    /// <summary>
    ///     The <see cref="IAnsiConsole"/> available for this execution.
    /// </summary>
    protected IAnsiConsole Console => FeatureCollection.Get<IConsoleFeature>()?.Console ?? Console;

    /// <summary>
    ///     A <see cref="CancellationToken"/> to abort any asynchronous operation.
    /// </summary>
    protected CancellationToken CancellationToken { get; private set; }

    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        FeatureCollection = context.Data as IFeatureCollection ?? new NullFeatureCollection();
        CancellationToken = cancellationToken;

        await ExecuteAsync(settings);

        return 0;
    }

    protected abstract Task ExecuteAsync(TSettings settings);

    private sealed class NullFeatureCollection : IFeatureCollection
    {
        public TFeature? Get<TFeature>() where TFeature : class
        {
            return null;
        }

        public void Set<TFeature>(TFeature? feature) where TFeature : class
        {
            throw new NotSupportedException("Setting features here is not allowed.");
        }
    }

    private sealed class NullRegistry : IRegistry
    {
        public IDocumentRewriterFactory DocumentRewriter => throw new NotSupportedException();
        public ITargetedRewriterFactory TargetedRewriter => throw new NotSupportedException();
        public ISymbolFilterFactory SymbolFilter => throw new NotSupportedException();
    }
}
