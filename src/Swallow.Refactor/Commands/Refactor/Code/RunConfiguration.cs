namespace Swallow.Refactor.Commands.Refactor.Code;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Abstractions;
using Abstractions.Filtering;
using Abstractions.Rewriting;
using Core.DocumentFilters;

public class RunConfiguration
{
    private RunConfiguration(IReadOnlyCollection<IDocumentFilter> documentFilters, IReadOnlyCollection<IDocumentRewriter> rewriters)
    {
        DocumentFilters = documentFilters;
        Rewriters = rewriters;
    }

    public IReadOnlyCollection<IDocumentFilter> DocumentFilters { get; }
    public IReadOnlyCollection<IDocumentRewriter> Rewriters { get; }

    public static RunConfiguration For(
        string? fileNameRegex,
        string? content,
        IEnumerable<string> rewriterExpressions,
        IDocumentRewriterFactory documentRewriterFactory)
    {
        var filters = GetFiltersFor(fileNameRegex: fileNameRegex, content: content);
        var rewriters = rewriterExpressions.Select(ParseExpression)
            .Select(t => documentRewriterFactory.Create(name: t.Name, parameters: t.Parameters))
            .ToList();

        return new(documentFilters: filters, rewriters: rewriters);
    }

    public static RunConfiguration For(Recipe recipe, IDocumentRewriterFactory documentRewriterFactory)
    {
        var filters = GetFiltersFor(fileNameRegex: recipe.Filter.Name, content: recipe.Filter.Content);
        var rewriters = recipe.Rewriters.Select(t => documentRewriterFactory.Create(name: t.Name, parameters: t.Parameters)).ToList();

        return new(documentFilters: filters, rewriters: rewriters);
    }

    private static (string Name, string[] Parameters) ParseExpression(string expression)
    {
        var parsedExpression = SyntaxFactory.ParseExpression(expression);

        return parsedExpression switch
        {
            IdentifierNameSyntax identifier => (identifier.Identifier.Text, Array.Empty<string>()),
            InvocationExpressionSyntax invocation => (invocation.Expression.ToString(),
                invocation.ArgumentList.Arguments.Select(a => a.Expression.ToString()).ToArray()),
            _ => throw new InvalidOperationException(
                $"Failed to parse rewriter-expression {expression}; only names (like \"MyRewriter\") or invocations (like \"MyRewriter(\"param\", \"param\")\") are allowed.")
        };
    }

    private static IReadOnlyCollection<IDocumentFilter> GetFiltersFor(string? fileNameRegex, string? content)
    {
        var filters = new List<IDocumentFilter>();
        if (fileNameRegex is not null)
        {
            filters.Add(new FilepathFilter(new(fileNameRegex)));
        }

        if (content is not null)
        {
            filters.Add(new ContentFilter(content));
        }

        if (filters.Count == 0)
        {
            filters.Add(new MatchAllFilter());
        }

        return filters;
    }

    private sealed class MatchAllFilter : IDocumentFilter
    {
        public Task<bool> Matches(Document document)
        {
            return Task.FromResult(true);
        }
    }
}
