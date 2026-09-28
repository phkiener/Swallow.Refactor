namespace Swallow.Refactor.Core.DocumentFilters;

using System.Text.RegularExpressions;
using Testing;

internal sealed partial class FilepathFilterTest : RoslynTest
{
    [GeneratedRegex(@"^(.*/)?impl/.+\.cs$")]
    private static partial Regex TestRegex();

    [Test]
    public async Task ReturnsTrue_WhenFilepathMatchesRegex()
    {
        var document = AddDocument(fileName: "path/impl/file.cs", sourceCode: "namespace Stuff;");
        var filter = new FilepathFilter(TestRegex());
        var result = await filter.Matches(document);
        Assert.That(actual: result, expression: Is.True);
    }

    [Test]
    public async Task ReturnsFalse_WhenFilepathDoesNotMatchRegex()
    {
        var document = AddDocument(fileName: "path/file.cs", sourceCode: "namespace Stuff;");
        var filter = new FilepathFilter(TestRegex());
        var result = await filter.Matches(document);
        Assert.That(actual: result, expression: Is.False);
    }
}
