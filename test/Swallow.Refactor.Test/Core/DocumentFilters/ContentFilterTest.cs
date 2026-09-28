namespace Swallow.Refactor.Core.DocumentFilters;

using Testing;

internal sealed class ContentFilterTest : RoslynTest
{
    [Test]
    public async Task ReturnsTrue_WhenDocumentContainsText()
    {
        var document = AddDocument("namespace Stuff;");
        var filter = new ContentFilter("Stuff");
        var result = await filter.Matches(document);
        Assert.That(actual: result, expression: Is.True);
    }

    [Test]
    public async Task ReturnsFalse_WhenDocumentDoesNotContainText()
    {
        var document = AddDocument("namespace Stuff;");
        var filter = new ContentFilter("Shizzle");
        var result = await filter.Matches(document);
        Assert.That(actual: result, expression: Is.False);
    }
}
