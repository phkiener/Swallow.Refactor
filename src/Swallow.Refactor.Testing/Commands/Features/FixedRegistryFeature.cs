namespace Swallow.Refactor.Testing.Commands.Features;

using Abstractions;
using Execution.Features;

/// <summary>
///     A feature that will provide a fixed registry.
/// </summary>
/// <param name="registry">The registry to use.</param>
public sealed class FixedRegistryFeature(IRegistry registry) : IRegistryFeature
{
    /// <inheritdoc />
    public IRegistry Registry { get; } = registry;
}
