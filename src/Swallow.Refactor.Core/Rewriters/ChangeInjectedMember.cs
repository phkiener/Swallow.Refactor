namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Modify;
using Query;

[Description(
    "Change the type and name of a member that is injected. It will be replaced in-place; the single constructor and all usages of the member are adjusted.")]
public sealed class ChangeInjectedMember : SemanticDocumentRewriter
{
    private readonly string sourceTypeName;
    private readonly string targetTypeName;
    private readonly string targetMemberName;

    public ChangeInjectedMember(
        [Description("Type of the member to replace")] string sourceTypeName,
        [Description("Type for the replacing member")] string targetTypeName,
        [Description("Name of the replacing member")] string targetMemberName)
    {
        this.sourceTypeName = sourceTypeName;
        this.targetTypeName = targetTypeName;
        this.targetMemberName = targetMemberName;
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        foreach (var classDeclaration in RootNode.DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>())
        {
            foreach (var field in classDeclaration.GetFieldsWithType(sourceTypeName))
            {
                documentEditor.ChangeField(field: field, typeName: targetTypeName, variableName: targetMemberName);
                var fieldSymbol = SemanticModel.GetDeclaredSymbol(field.Declaration.Variables.Single());
                documentEditor.RenameReferencesTo(symbol: fieldSymbol!, name: targetMemberName, semanticModel: SemanticModel);
            }

            var constructor = classDeclaration.GetSingleConstructor();
            if (constructor is null)
            {
                continue;
            }

            foreach (var parameter in constructor.GetParametersOfType(sourceTypeName))
            {
                documentEditor.ChangeParameter(parameter: parameter, typeName: targetTypeName, variableName: targetMemberName);
                var parameterSymbol = SemanticModel.GetDeclaredSymbol(parameter);
                documentEditor.RenameReferencesTo(symbol: parameterSymbol!, name: targetMemberName, semanticModel: SemanticModel);
            }
        }
    }
}
