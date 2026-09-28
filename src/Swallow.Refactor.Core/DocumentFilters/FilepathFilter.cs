namespace Swallow.Refactor.Core.DocumentFilters;

using System.Text.RegularExpressions;
using Abstractions.Filtering;
using Microsoft.CodeAnalysis;

public sealed class FilepathFilter : IDocumentFilter
{
    private readonly Regex filePathRegex;

    public FilepathFilter(Regex filePathRegex)
    {
        this.filePathRegex = filePathRegex;
    }

    public Task<bool> Matches(Document document)
    {
        var isMatch = filePathRegex.IsMatch(document.FilePath ?? "");

        return Task.FromResult(isMatch);
    }
}
