namespace Swallow.Refactor.Commands.Interactive;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal abstract record Command
{
    public sealed record Quit : Command;
    public sealed record List : Command;
    public sealed record Help : Command;
    public sealed record Describe(string Name) : Command;
    public sealed record Rewriter(string Name, string[] Parameters) : Command;
}

internal static class CommandParser
{
    public static Command? Parse(string input)
    {
        return input.StartsWith(".") ? ParseMetaCommand(input[1..]) : ParseExpression(input);
    }

    private static Command? ParseMetaCommand(string input)
    {
        return input.Split(' ') switch
        {
            ["quit"] => new Command.Quit(),
            ["exit"] => new Command.Quit(),
            ["help"] => new Command.Help(),
            ["list"] => new Command.List(),
            ["describe", var name] => new Command.Describe(name),
            _ => null,
        };
    }

    private static Command.Rewriter? ParseExpression(string expression)
    {
        var parsedExpression = SyntaxFactory.ParseExpression(expression);

        return parsedExpression switch
        {
            IdentifierNameSyntax identifier => new(identifier.Identifier.Text, Array.Empty<string>()),
            InvocationExpressionSyntax invocation => new (invocation.Expression.ToString(), invocation.ArgumentList.Arguments.Select(a => a.Expression.ToString()).ToArray()),
            _ => null
        };
    }
}
