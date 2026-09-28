namespace Swallow.Refactor.Core.DocumentFilters;

using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

public sealed class ContentFilter : IDocumentFilter
{
    private readonly string textToFind;

    public ContentFilter(string textToFind)
    {
        this.textToFind = textToFind;
    }

    public async Task<bool> Matches(Document document)
    {
        var content = await document.GetTextAsync();

        return content.ToString().Contains(textToFind);
    }
}
