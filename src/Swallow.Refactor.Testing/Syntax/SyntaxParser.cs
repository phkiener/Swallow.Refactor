namespace Swallow.Refactor.Testing.Syntax;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class SyntaxParser
{
    private static readonly IReadOnlyDictionary<Type, Func<string, SyntaxNode?>> parserByType = new Dictionary<Type, Func<string, SyntaxNode?>>()
    {
        [typeof(ExpressionSyntax)] = s => SyntaxFactory.ParseExpression(s),
        [typeof(MemberDeclarationSyntax)] = s => SyntaxFactory.ParseMemberDeclaration(s),
        [typeof(CompilationUnitSyntax)] = s => SyntaxFactory.ParseCompilationUnit(s),
        [typeof(ParameterListSyntax)] = s => SyntaxFactory.ParseParameterList(s)
    };

    /// <summary>
    ///     Parse a piece of code into the given type of syntax node.
    /// </summary>
    /// <param name="code">The code to parse.</param>
    /// <typeparam name="T">Type of syntax to parse.</typeparam>
    /// <returns>The parsed node.</returns>
    /// <exception cref="NotImplementedException">The type of syntax node is not supported (yet).</exception>
    public static T ParseAs<T>([StringSyntax("C#")] this string code) where T : SyntaxNode
    {
        foreach (var (type, parser) in parserByType)
        {
            if (typeof(T).IsAssignableTo(type))
            {
                return parser.Invoke(code).CastOrThrow<T>();
            }
        }

        throw new NotImplementedException($"Parsing nodes of type {typeof(T).Name} is not supported yet.");
    }

    private static T CastOrThrow<T>(this object? input)
    {
        return input is T casted ? casted : throw new InvalidOperationException($"Cannot parse given code as {typeof(T).Name}");
    }
}
