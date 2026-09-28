namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

[Description("Replace a chunk of code with another chunk of code.")]
public sealed class ReplaceCode : SyntaxDocumentRewriter
{
    private readonly string source;
    private readonly string replacement;

    public ReplaceCode([Description("Code to be replaced")] string source, [Description("Replacement code to be inserted")] string replacement)
    {
        this.source = source;
        this.replacement = replacement;
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        var replacementExpression = SyntaxFactory.ParseExpression(replacement);
        foreach (var expressionSyntax in RootNode.DescendantNodes(_ => true).OfType<ExpressionSyntax>())
        {
            if (expressionSyntax.ToString() == source)
            {
                documentEditor.ReplaceNode(
                    node: expressionSyntax,
                    newNode: replacementExpression.WithLeadingTrivia(expressionSyntax.GetLeadingTrivia())
                        .WithTrailingTrivia(expressionSyntax.GetTrailingTrivia()));
            }
        }
    }
}
