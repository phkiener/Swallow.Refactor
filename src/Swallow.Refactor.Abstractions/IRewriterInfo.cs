namespace Swallow.Refactor.Abstractions;

/// <summary>
///     General information about a rewriter.
/// </summary>
public interface IRewriterInfo
{
    /// <summary>
    ///     Name of the rewriter.
    /// </summary>
    /// <remarks>
    ///     The rewriter can be invoked using this name.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Description on the rewriters function.
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     Parameters that are required for the rewriter.
    /// </summary>
    IReadOnlyCollection<IRewriterParameterInfo> Parameters { get; }
}

/// <summary>
///     Information about a parameter of a rewriter.
/// </summary>
public interface IRewriterParameterInfo
{
    /// <summary>
    ///     The parameter's name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Description of the parameter.
    /// </summary>
    string? Description { get; }
}
