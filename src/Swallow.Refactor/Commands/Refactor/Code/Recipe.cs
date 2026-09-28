namespace Swallow.Refactor.Commands.Refactor.Code;

public sealed record Recipe(Recipe.DocumentFilter Filter, Recipe.Rewriter[] Rewriters)
{
    public sealed record DocumentFilter(string? Name, string? Content);
    public sealed record Rewriter(string Name, string[] Parameters);
}
