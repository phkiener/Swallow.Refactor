namespace Swallow.Refactor.Core;

using System.ComponentModel;
using System.Reflection;
using Abstractions;
using Abstractions.Filtering;
using Abstractions.Rewriting;

/// <summary>
///     A <see cref="IRegistry"/> that will discover all relevant classes from given assemblies via reflection.
/// </summary>
public sealed class ReflectionRegistry : IRegistry
{
    private readonly Dictionary<string, RewriterInfo> documentRewriterByName;
    private readonly Dictionary<string, RewriterInfo> targetedRewriterByName;
    private readonly Dictionary<string, SymbolFilterInfo> symbolFilterByName;

    private ReflectionRegistry(
        IEnumerable<RewriterInfo> documentRewriters,
        IEnumerable<RewriterInfo> targetedRewriters,
        IEnumerable<SymbolFilterInfo> symbolFilters)
    {
        symbolFilterByName = symbolFilters.ToDictionary(r => r.Name);
        documentRewriterByName = documentRewriters.ToDictionary(r => r.Name);
        targetedRewriterByName = targetedRewriters.ToDictionary(r => r.Name);

        DocumentRewriter = new ProxyDocumentRewriterFactory(this);
        TargetedRewriter = new ProxyTargetedRewriterFactory(this);
        SymbolFilter = new ProxySymbolFilterFactory(this);
    }

    /// <inheritdoc />
    public IDocumentRewriterFactory DocumentRewriter { get; }

    public ITargetedRewriterFactory TargetedRewriter { get; }

    /// <inheritdoc />
    public ISymbolFilterFactory SymbolFilter { get; }

    /// <summary>
    ///     Create the registry.
    /// </summary>
    /// <param name="assemblies">The assemblies to analyze.</param>
    /// <returns>The created and fully initialized registry.</returns>
    public static IRegistry CreateFrom(params Assembly[] assemblies)
    {
        var allDocumentRewriters = assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsAssignableTo(typeof(IDocumentRewriter)) && t is { IsAbstract: false, IsGenericType: false, IsInterface: false })
            .Select(t => new RewriterInfo(t.GetConstructors().Single()));

        var allTargetedRewriters = assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsAssignableTo(typeof(ITargetedRewriter)) && t is { IsAbstract: false, IsGenericType: false, IsInterface: false })
            .Select(t => new RewriterInfo(t.GetConstructors().Single()));

        var allSymbolFilters = assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsAssignableTo(typeof(ISymbolFilter)) && t is { IsAbstract: false, IsGenericType: false, IsInterface: false })
            .Select(t => new SymbolFilterInfo(t));

        return new ReflectionRegistry(documentRewriters: allDocumentRewriters, targetedRewriters: allTargetedRewriters, symbolFilters: allSymbolFilters);
    }

    private sealed class ProxyDocumentRewriterFactory : IDocumentRewriterFactory
    {
        private readonly ReflectionRegistry parentRegistry;

        public ProxyDocumentRewriterFactory(ReflectionRegistry parentRegistry)
        {
            this.parentRegistry = parentRegistry;
        }

        public IDocumentRewriter Create(string name, params string[] parameters)
        {
            var rewriter = parentRegistry.documentRewriterByName.GetValueOrDefault(name)
                           ?? throw new KeyNotFoundException($"No rewriter called '{name}' is registered.");

            var rewriterParameterTypes = rewriter.Parameters.Select(p => p.ParameterInfo.ParameterType).ToList();
            if (rewriterParameterTypes.Count == 1 && rewriterParameterTypes.Single() == typeof(string[]))
            {
                return (IDocumentRewriter)rewriter.ConstructorInfo.Invoke(new object?[] { parameters });
            }

            return (IDocumentRewriter)rewriter.ConstructorInfo.Invoke(parameters.Zip(second: rewriterParameterTypes, resultSelector: TryParse).ToArray());
        }

        private static object? TryParse(string value, Type type)
        {
            if (type == typeof(string))
            {
                return value;
            }

            var parseMethod = type.GetMethod(
                name: nameof(int.Parse),
                bindingAttr: BindingFlags.Public | BindingFlags.Static,
                types: new[] { typeof(string) });

            return parseMethod is not null
                ? parseMethod.Invoke(obj: null, parameters: new object?[] { value })
                : Convert.ChangeType(value: value, conversionType: type);
        }

        public IReadOnlyCollection<IRewriterInfo> List()
        {
            return parentRegistry.documentRewriterByName.Values.OrderBy(r => r.Name).ToList();
        }
    }

    private sealed class ProxyTargetedRewriterFactory : ITargetedRewriterFactory
    {
        private readonly ReflectionRegistry parentRegistry;

        public ProxyTargetedRewriterFactory(ReflectionRegistry parentRegistry)
        {
            this.parentRegistry = parentRegistry;
        }

        public ITargetedRewriter Create(string name, params string[] parameters)
        {
            var rewriter = parentRegistry.targetedRewriterByName.GetValueOrDefault(name)
                           ?? throw new KeyNotFoundException($"No targeted rewriter called '{name}' is registered.");

            var rewriterParameterTypes = rewriter.Parameters.Select(p => p.ParameterInfo.ParameterType).ToList();
            if (rewriterParameterTypes.Count == 1 && rewriterParameterTypes.Single() == typeof(string[]))
            {
                return (ITargetedRewriter)rewriter.ConstructorInfo.Invoke(new object?[] { parameters });
            }

            return (ITargetedRewriter)rewriter.ConstructorInfo.Invoke(parameters.Zip(second: rewriterParameterTypes, resultSelector: TryParse).ToArray());
        }

        private static object? TryParse(string value, Type type)
        {
            if (type == typeof(string))
            {
                return value;
            }

            var parseMethod = type.GetMethod(
                name: nameof(int.Parse),
                bindingAttr: BindingFlags.Public | BindingFlags.Static,
                types: new[] { typeof(string) });

            return parseMethod is not null
                ? parseMethod.Invoke(obj: null, parameters: new object?[] { value })
                : Convert.ChangeType(value: value, conversionType: type);
        }

        public IReadOnlyCollection<IRewriterInfo> List()
        {
            return parentRegistry.targetedRewriterByName.Values.OrderBy(r => r.Name).ToList();
        }
    }

    private sealed class ProxySymbolFilterFactory : ISymbolFilterFactory
    {
        private readonly ReflectionRegistry parentRegistry;

        public ProxySymbolFilterFactory(ReflectionRegistry parentRegistry)
        {
            this.parentRegistry = parentRegistry;
        }

        public ISymbolFilter Create(string name)
        {
            var symbolFilter = parentRegistry.symbolFilterByName.GetValueOrDefault(name)
                               ?? throw new KeyNotFoundException($"No symbol filter called '{name}' is registered.");

            return (ISymbolFilter)Activator.CreateInstance(symbolFilter.Type)!;
        }

        public IReadOnlyCollection<ISymbolFilterInfo> List()
        {
            return parentRegistry.symbolFilterByName.Values.OrderBy(s => s.Name).ToList();
        }
    }

    private sealed record RewriterInfo(ConstructorInfo ConstructorInfo) : IRewriterInfo
    {
        private Type Type => ConstructorInfo.DeclaringType!;
        public string Name => Type.Name;
        public string? Description => Type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        public IReadOnlyCollection<Parameter> Parameters => ConstructorInfo.GetParameters().Select(p => new Parameter(p)).ToArray();

        IReadOnlyCollection<IRewriterParameterInfo> IRewriterInfo.Parameters
            => ConstructorInfo.GetParameters().Select(p => new Parameter(p)).ToArray();

        public sealed record Parameter(ParameterInfo ParameterInfo) : IRewriterParameterInfo
        {
            public string Name => ParameterInfo.Name!;
            public string? Description => ParameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }

    private sealed record SymbolFilterInfo(Type Type) : ISymbolFilterInfo
    {
        public string Name => Type.Name;
        public string? Description => Type.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}
