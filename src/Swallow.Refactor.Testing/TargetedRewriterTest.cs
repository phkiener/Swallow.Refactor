namespace Swallow.Refactor.Testing;

using Abstractions.Rewriting;

public abstract class TargetedRewriterTest : RoslynTest
{
    protected abstract ITargetedRewriter TargetedRewriter { get; }

    protected async Task RunRewriterAsync(ISymbol symbol)
    {
        var editor = new SolutionEditor(CurrentSolution);
        await TargetedRewriter.RunAsync(editor, symbol);

        var changedSolution = editor.GetChangedSolution();
        Workspace.TryApplyChanges(changedSolution);
    }
}
