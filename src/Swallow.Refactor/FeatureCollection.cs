namespace Swallow.Refactor;

using Execution;

public sealed class FeatureCollection : IFeatureCollection, IAsyncDisposable
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

    public async ValueTask DisposeAsync()
    {
        foreach (var feature in featureInstances.Values)
        {
            if (feature is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (feature is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
