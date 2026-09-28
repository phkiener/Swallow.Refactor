namespace Swallow.Refactor.Execution;

/// <summary>
///     A collection of (singleton) features similar to the ASP.NET core feature collection.
/// </summary>
public interface IFeatureCollection
{
    /// <summary>
    ///     Retrieve the current instance of the given feature type.
    /// </summary>
    /// <typeparam name="TFeature">Type of the feature.</typeparam>
    /// <returns>The current instance of the feature or <c>null</c>.</returns>
    TFeature? Get<TFeature>() where TFeature : class;

    /// <summary>
    ///     Set the current instance of a feature type.
    /// </summary>
    /// <param name="feature">The feature instance to set.</param>
    /// <typeparam name="TFeature">Type of the feature.</typeparam>
    void Set<TFeature>(TFeature? feature) where TFeature : class;
}
