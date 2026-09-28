namespace Swallow.Refactor.Core.Query;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class TypeDeclarationQueries
{
    public static IEnumerable<FieldDeclarationSyntax> GetFieldsWithType(this TypeDeclarationSyntax typeDeclarationSyntax, string typeName)
    {
        return typeDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>()
            .Where(f => (f.Declaration.Type as SimpleNameSyntax)?.Identifier.Text == typeName);
    }

    public static FieldDeclarationSyntax? GetField(this TypeDeclarationSyntax typeDeclarationSyntax, string fieldName)
    {
        return typeDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>()
            .SingleOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == fieldName));
    }

    public static ConstructorDeclarationSyntax? GetSingleConstructor(this TypeDeclarationSyntax typeDeclarationSyntax)
    {
        var constructors = typeDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>().ToList();

        return constructors.Count == 1 ? constructors.Single() : null;
    }
}
