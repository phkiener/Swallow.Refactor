namespace Swallow.Refactor.Core.Modify;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

public static class SyntaxEditorExtensions
{
    public static void RecordChange<T, TOut>(this SyntaxEditor editor, T node, Func<T, TOut> transformer)
        where T : SyntaxNode
        where TOut : SyntaxNode
    {
        editor.ReplaceNode(node: node, computeReplacement: (n, _) => transformer((T)n));
    }

    public static void RecordChanges<T, TOut>(this SyntaxEditor editor, IEnumerable<T> nodes, Func<T, TOut> transformer)
        where T : SyntaxNode
        where TOut : SyntaxNode
    {
        foreach (var n in nodes)
        {
            RecordChange(editor, n, transformer);
        }
    }
}
