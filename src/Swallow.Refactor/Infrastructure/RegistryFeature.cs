namespace Swallow.Refactor.Infrastructure;

using System.Reflection;
using Abstractions;
using Core;
using Execution;
using Execution.Features;
using Spectre.Console.Cli;

public sealed class RegistryFeature : IRegistryFeature, ICommandInterceptor
{
    private readonly Assembly[] assemblies;
    private IRegistry? registry;
    public IRegistry Registry => registry ?? throw new InvalidOperationException("No registry was loaded.");

    public RegistryFeature(IEnumerable<Assembly> assemblies)
    {
        this.assemblies = assemblies.ToArray();
    }

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        if (context.Data is not IFeatureCollection featureCollection)
        {
            return;
        }

        registry = ReflectionRegistry.CreateFrom(assemblies);
        featureCollection.Set<IRegistryFeature>(this);
    }
}
