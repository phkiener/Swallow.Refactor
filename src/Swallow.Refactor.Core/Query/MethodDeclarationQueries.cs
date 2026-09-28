namespace Swallow.Refactor.Core.Query;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class MethodDeclarationQueries
{
    public static IEnumerable<ParameterSyntax> GetParametersOfType(this BaseMethodDeclarationSyntax baseMethodDeclarationSyntax, string typeName)
    {
        return baseMethodDeclarationSyntax.ParameterList.Parameters.Where(p => (p.Type as SimpleNameSyntax)?.Identifier.Text == typeName);
    }
}
