namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Modify;

[Description("Rename all occurrances of an identifier")]
public class RenameIdentifier : SyntaxDocumentRewriter
{
    private readonly string currentIdentifier;
    private readonly string newIdentifier;

    public RenameIdentifier(
        [Description("Original identifier to replace")] string currentIdentifier,
        [Description("New name for the identifier")] string newIdentifier)
    {
        this.currentIdentifier = currentIdentifier;
        this.newIdentifier = newIdentifier;
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        foreach (var identifierNameSyntax in RootNode.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (identifierNameSyntax.Identifier.Text == currentIdentifier)
            {
                var replacementIdentifier = SyntaxFactory.Identifier(newIdentifier)
                    .WithLeadingTrivia(identifierNameSyntax.Identifier.LeadingTrivia)
                    .WithTrailingTrivia(identifierNameSyntax.Identifier.TrailingTrivia);

                documentEditor.RecordChange(node: identifierNameSyntax, transformer: i => i.WithIdentifier(replacementIdentifier));
            }
        }
    }
}
