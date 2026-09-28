namespace Swallow.Refactor.Testing.Assertion;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Assertions to check two pieces of code.
/// </summary>
/// <remarks>
///     All assertions are whitespace-sensitive but will normalize line endings to LF.
/// </remarks>
public static class SyntaxAssert
{
    /// <summary>
    ///     Check that the full text of two <see cref="SyntaxNode"/>s is equal.
    /// </summary>
    /// <param name="expected">Expected syntax node.</param>
    /// <param name="actual">Actual syntax node.</param>
    public static void AreEqual(SyntaxNode expected, SyntaxNode actual)
    {
        AreEqual(actual: SourceText.From(actual.ToFullString()), expected: SourceText.From(expected.ToFullString()));
    }

    /// <summary>
    ///     Check that the full text of the given <see cref="SyntaxNode"/> matches the given source text.
    /// </summary>
    /// <param name="expected">Expected source text.</param>
    /// <param name="actual">Actual syntax node.</param>
    public static void AreEqual([StringSyntax("C#")] string expected, SyntaxNode actual)
    {
        AreEqual(actual: SourceText.From(actual.ToFullString()), expected: SourceText.From(expected));
    }

    /// <summary>
    ///     Check that the given source text matches the full text of the expected <see cref="SyntaxNode"/>.
    /// </summary>
    /// <param name="expected">Expected syntax node.</param>
    /// <param name="actual">Actual source text.</param>
    public static void AreEqual(SyntaxNode expected, [StringSyntax("C#")] string actual)
    {
        AreEqual(actual: SourceText.From(actual), expected: SourceText.From(expected.ToFullString()));
    }

    /// <inheritdoc cref="AreEqual(Microsoft.CodeAnalysis.Text.SourceText,Microsoft.CodeAnalysis.Text.SourceText)"/>
    public static void AreEqual([StringSyntax("C#")] string expected, [StringSyntax("C#")] string actual)
    {
        AreEqual(actual: SourceText.From(actual), expected: SourceText.From(expected));
    }

    /// <inheritdoc cref="AreEqual(Microsoft.CodeAnalysis.Text.SourceText,Microsoft.CodeAnalysis.Text.SourceText)"/>
    public static void AreEqual(SourceText expected, [StringSyntax("C#")] string actual)
    {
        AreEqual(actual: SourceText.From(actual), expected: expected);
    }

    /// <inheritdoc cref="AreEqual(Microsoft.CodeAnalysis.Text.SourceText,Microsoft.CodeAnalysis.Text.SourceText)"/>
    public static void AreEqual([StringSyntax("C#")] string expected, SourceText actual)
    {
        AreEqual(actual: actual, expected: SourceText.From(expected));
    }

    /// <summary>
    ///     Check that two source texts are equal.
    /// </summary>
    /// <param name="expected">Expected source text.</param>
    /// <param name="actual">Actual source text.</param>
    public static void AreEqual(SourceText actual, SourceText expected)
    {
        var actualText = actual.ToString().Replace(oldValue: "\r\n", newValue: "\n");
        var expectedText = expected.ToString().Replace(oldValue: "\r\n", newValue: "\n");
        Assert.That(actual: actualText, expression: Is.EqualTo(expectedText));
    }
}
