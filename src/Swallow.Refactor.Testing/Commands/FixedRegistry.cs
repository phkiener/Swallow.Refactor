namespace Swallow.Refactor.Testing.Commands;

using System.Reflection;
using Abstractions;
using Abstractions.Filtering;
using Abstractions.Rewriting;

public sealed class FixedRegistry : IRegistry, IDocumentRewriterFactory, ITargetedRewriterFactory, ISymbolFilterFactory
{
    private readonly List<RewriterInfo> rewriters = new();
    private readonly List<RewriterInfo> targetedRewriters = new();
    private readonly List<SymbolFilterInfo> symbolFilters = new();

    public IDocumentRewriterFactory DocumentRewriter => this;
    public ITargetedRewriterFactory TargetedRewriter => this;
    public ISymbolFilterFactory SymbolFilter => this;

    public FixedRegistry IncludeRewriter<TRewriter>() where TRewriter : IDocumentRewriter
    {
        rewriters.Add(new(typeof(TRewriter).GetConstructors().Single()));

        return this;
    }

    public FixedRegistry IncludeTargetedRewriter<TRewriter>() where TRewriter : ITargetedRewriter
    {
        targetedRewriters.Add(new(typeof(TRewriter).GetConstructors().Single()));

        return this;
    }

    public FixedRegistry IncludeSymbolFilter<TSymbolFilter>() where TSymbolFilter : ISymbolFilter
    {
        symbolFilters.Add(new(typeof(TSymbolFilter)));

        return this;
    }

    IReadOnlyCollection<IRewriterInfo> IDocumentRewriterFactory.List()
    {
        return rewriters;
    }

    IReadOnlyCollection<IRewriterInfo> ITargetedRewriterFactory.List()
    {
        return targetedRewriters;
    }

    IReadOnlyCollection<ISymbolFilterInfo> ISymbolFilterFactory.List()
    {
        return symbolFilters;
    }

    IDocumentRewriter IDocumentRewriterFactory.Create(string name, params string[] parameters)
    {
        var rewriter = rewriters.Single(r => r.Name == name);

        return (IDocumentRewriter)rewriter.ConstructorInfo.Invoke(parameters.Cast<object?>().ToArray());
    }

    ITargetedRewriter ITargetedRewriterFactory.Create(string name, params string[] parameters)
    {
        var rewriter = targetedRewriters.Single(r => r.Name == name);

        return (ITargetedRewriter)rewriter.ConstructorInfo.Invoke(parameters.Cast<object?>().ToArray());
    }

    ISymbolFilter ISymbolFilterFactory.Create(string name)
    {
        var symbolFilter = symbolFilters.Single(r => r.Name == name);

        return (ISymbolFilter)Activator.CreateInstance(symbolFilter.Type)!;
    }

    private sealed record RewriterInfo(ConstructorInfo ConstructorInfo) : IRewriterInfo
    {
        private Type Type => ConstructorInfo.DeclaringType!;
        public string Name => Type.Name;
        public string? Description => Type.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

        IReadOnlyCollection<IRewriterParameterInfo> IRewriterInfo.Parameters
            => ConstructorInfo.GetParameters().Select(p => new Parameter(p)).ToArray();

        private sealed record Parameter(ParameterInfo ParameterInfo) : IRewriterParameterInfo
        {
            public string Name => ParameterInfo.Name!;
            public string? Description => ParameterInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;
        }
    }

    private sealed record SymbolFilterInfo(Type Type) : ISymbolFilterInfo
    {
        public string Name => Type.Name;
        public string? Description => Type.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;
    }
}
