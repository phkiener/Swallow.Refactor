namespace Swallow.Refactor.Testing.Commands;

using Execution;

internal sealed class TestFeatureCollection : IFeatureCollection
{
    private readonly IDictionary<Type, object?> featureInstances = new Dictionary<Type, object?>();

    public TFeature? Get<TFeature>() where TFeature : class
    {
        return featureInstances.TryGetValue(typeof(TFeature), out var instance) ? (TFeature?)instance : null;
    }

    public void Set<TFeature>(TFeature? feature) where TFeature : class
    {
        featureInstances[typeof(TFeature)] = feature;
    }
}
