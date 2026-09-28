namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Modify;

[Description("Add a single using to the document. If the using is already present, nothing is done.")]
public sealed class AddUsing : SyntaxDocumentRewriter
{
    private readonly string @namespace;

    public AddUsing([Description("The namespace to add")] string @namespace)
    {
        this.@namespace = @namespace;
    }

    protected override void Run(DocumentEditor documentEditor)
    {
        documentEditor.RecordChange(node: (CompilationUnitSyntax)RootNode, transformer: c => c.AddUsing(@namespace));
    }
}
