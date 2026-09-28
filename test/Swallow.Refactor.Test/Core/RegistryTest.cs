namespace Swallow.Refactor.Core;

using System.Reflection;
using Abstractions;
using Abstractions.Rewriting;

internal sealed class RegistryTest
{
    private static IRegistry Registry => ReflectionRegistry.CreateFrom(Assembly.GetExecutingAssembly());

    [Test]
    public void CanProduceParameterlessRewriter()
    {
        var rewriter = Registry.DocumentRewriter.Create(nameof(ParameterlessDocumentRewriter));
        Assert.That(actual: rewriter, expression: Is.InstanceOf<ParameterlessDocumentRewriter>());
    }

    [Test]
    public void CanProducteRewriterWithStringParameters()
    {
        var rewriter = Registry.DocumentRewriter.Create(name: nameof(StringParameterDocumentRewriter), "first", "second");
        Assert.That(actual: rewriter, expression: Is.InstanceOf<StringParameterDocumentRewriter>());
        var typedRewriter = rewriter as StringParameterDocumentRewriter;
        Assert.That(actual: typedRewriter?.First, expression: Is.EqualTo("first"));
        Assert.That(actual: typedRewriter?.Second, expression: Is.EqualTo("second"));
    }

    [Test]
    public void CanParseParametersWhoseTypeHasAStaticParseMethod()
    {
        var rewriter = Registry.DocumentRewriter.Create(name: nameof(ParseableTypeParameterDocumentRewriter), "11");
        Assert.That(actual: rewriter, expression: Is.InstanceOf<ParseableTypeParameterDocumentRewriter>());
        var typedRewriter = rewriter as ParseableTypeParameterDocumentRewriter;
        Assert.That(actual: typedRewriter?.Number, expression: Is.EqualTo(11));
    }

    [Test]
    public void CanParseParametersWhichAreUsedForParamsArray()
    {
        var rewriter = Registry.DocumentRewriter.Create(name: nameof(ParamsParameterDocumentRewriter), "a", "b", "c");
        Assert.That(actual: rewriter, expression: Is.InstanceOf<ParamsParameterDocumentRewriter>());
        var typedRewriter = rewriter as ParamsParameterDocumentRewriter;
        Assert.That(actual: typedRewriter?.Parameters, expression: Is.EqualTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public void CanParseEmptyParametersForParamsArray()
    {
        var rewriter = Registry.DocumentRewriter.Create(name: nameof(ParamsParameterDocumentRewriter));
        Assert.That(actual: rewriter, expression: Is.InstanceOf<ParamsParameterDocumentRewriter>());
        var typedRewriter = rewriter as ParamsParameterDocumentRewriter;
        Assert.That(actual: typedRewriter?.Parameters, expression: Is.Empty);
    }

    [Test]
    public void CanListAllFoundRewriters()
    {
        var rewriters = Registry.DocumentRewriter.List();
        Assert.That(
            actual: rewriters,
            expression: Contains.Item(new SlimRewriterInfo(Name: nameof(ParameterlessDocumentRewriter), Parameters: Array.Empty<string>()))
                .Using<IRewriterInfo>(IsEquivalent));

        Assert.That(
            actual: rewriters,
            expression: Contains.Item(new SlimRewriterInfo(Name: nameof(StringParameterDocumentRewriter), Parameters: new[] { "first", "second" }))
                .Using<IRewriterInfo>(IsEquivalent));

        Assert.That(
            actual: rewriters,
            expression: Contains.Item(new SlimRewriterInfo(Name: nameof(ParseableTypeParameterDocumentRewriter), Parameters: new[] { "number" }))
                .Using<IRewriterInfo>(IsEquivalent));
    }

    [Test]
    public void CanProduceParameterlessTargetedRewriter()
    {
        var rewriter = Registry.TargetedRewriter.Create(nameof(TargetedRewriter));
        Assert.That(actual: rewriter, expression: Is.InstanceOf<TargetedRewriter>());
    }

    private static int IsEquivalent(IRewriterInfo left, IRewriterInfo right)
    {
        var areEqual = left.Name == right.Name
                       && left.Parameters.Count == right.Parameters.Count
                       && left.Parameters.Zip(right.Parameters).All(t => t.First.Name == t.Second.Name);

        return areEqual ? 0 : int.MaxValue;
    }

    private sealed record SlimRewriterInfo(string Name, string[] Parameters) : IRewriterInfo
    {
        public string? Description => null;
        IReadOnlyCollection<IRewriterParameterInfo> IRewriterInfo.Parameters => Parameters.Select(p => new SlimRewriterParameterInfo(p)).ToArray();
    }

    private sealed record SlimRewriterParameterInfo(string Name) : IRewriterParameterInfo
    {
        public string? Description => null;
    }
}

public class ParameterlessDocumentRewriter : IDocumentRewriter
{
    public Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }
}

public class StringParameterDocumentRewriter : IDocumentRewriter
{
    public string First { get; }
    public string Second { get; }

    public StringParameterDocumentRewriter(string first, string second)
    {
        First = first;
        Second = second;
    }

    public Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }
}

public class ParseableTypeParameterDocumentRewriter : IDocumentRewriter
{
    public int Number { get; }

    public ParseableTypeParameterDocumentRewriter(int number)
    {
        Number = number;
    }

    public Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }
}

public class ParamsParameterDocumentRewriter : IDocumentRewriter
{
    public string[] Parameters { get; }

    public ParamsParameterDocumentRewriter(params string[] parameters)
    {
        Parameters = parameters;
    }

    public Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }
}

public class TargetedRewriter : ITargetedRewriter
{
    public Task RunAsync(SolutionEditor solutionEditor, ISymbol target)
    {
        throw new NotImplementedException();
    }
}
