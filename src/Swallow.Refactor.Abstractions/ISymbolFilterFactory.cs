namespace Swallow.Refactor.Abstractions;

using Filtering;

public interface ISymbolFilterFactory
{
    /// <summary>
    ///     Create the <see cref="ISymbolFilter"/> with the given name.
    /// </summary>
    /// <param name="name">Name of the symbol filter to create.</param>
    /// <returns>The created symbol filter.</returns>
    ISymbolFilter Create(string name);

    /// <summary>
    ///     List all registered <see cref="ISymbolFilter"/>s.
    /// </summary>
    IReadOnlyCollection<ISymbolFilterInfo> List();
}

public interface ISymbolFilterInfo
{
    /// <summary>
    ///     Name of the symbol filter.
    /// </summary>
    /// <remarks>
    ///     The symbol filter can be invoked using this name.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Description on the symbol filters function.
    /// </summary>
    string? Description { get; }
}
