namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Modify;
using Query;

[Description("Replace accesses to a field member with a new chunk of code.")]
public sealed class ReplaceFieldMemberAccess : SemanticDocumentRewriter
{
    private readonly string fieldName;
    private readonly string accessedMember;
    private readonly string replacement;

    public ReplaceFieldMemberAccess(
        [Description("Name of the field whose member accesses to replace")] string fieldName,
        [Description("Name of the accessed member to replace with code")] string accessedMember,
        [Description("Code to replace the member access with")] string replacement)
    {
        this.fieldName = fieldName;
        this.accessedMember = accessedMember;
        this.replacement = replacement;
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        foreach (var classDeclaration in RootNode.DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>())
        {
            var field = classDeclaration.GetField(fieldName);
            if (field is null)
            {
                continue;
            }

            var fieldSymbol = SemanticModel.GetDeclaredSymbol(field.Declaration.Variables.Single());
            foreach (var memberAccess in classDeclaration.DescendantNodes(_ => true).OfType<MemberAccessExpressionSyntax>())
            {
                var accessedSymbol = SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                if (SymbolEqualityComparer.Default.Equals(x: fieldSymbol, y: accessedSymbol) && memberAccess.Name.Identifier.Text == accessedMember)
                {
                    documentEditor.ReplaceNodeWithCode(node: memberAccess, expression: replacement);
                }
            }
        }
    }
}
