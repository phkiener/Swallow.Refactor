namespace Swallow.Refactor.Core.Rewriters;

using System.ComponentModel;
using Abstractions.Rewriting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Modify;

[Description("Rename the declaration and all usages of a certain symbol.")]
public sealed class RenameSymbol : ITargetedRewriter
{
    private readonly string targetName;

    public RenameSymbol([Description("New name for the symbol")] string targetName)
    {
        this.targetName = targetName;
    }

    public async Task RunAsync(SolutionEditor solutionEditor, ISymbol target)
    {
        var references = await SymbolFinder.FindReferencesAsync(target, solutionEditor.OriginalSolution);
        var sourceLocations = references.SelectMany(r => r.Definition.Locations.Concat(r.Locations.Select(l => l.Location))).Where(l => l.IsInSource);
        foreach (var location in sourceLocations)
        {
            var document = solutionEditor.OriginalSolution.GetDocument(location.SourceTree);
            var documentEditor = await solutionEditor.GetDocumentEditorAsync(document?.Id);

            var node = documentEditor.OriginalRoot.FindNode(location.SourceSpan);
            documentEditor.RecordChange(node, n => n.Rename(targetName));
        }
    }
}
