namespace Swallow.Refactor.Testing;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     Base class for any test that uses Roslyn features - syntax nodes, a semantic model, documents, projects, ...
/// </summary>
/// <remarks>
///     Each test method will recieve a new <see cref="AdhocWorkspace"/> setup with a single project.
/// </remarks>
public abstract class RoslynTest
{
    private AdhocWorkspace workspace = null!;
    private ProjectId projectId = null!;

    /// <summary>
    ///     The workspace opened for the test.
    /// </summary>
    protected Workspace Workspace => workspace;

    /// <summary>
    ///     Shortcut for the currently loaded solution.
    /// </summary>
    protected Solution CurrentSolution => workspace.CurrentSolution;

    /// <summary>
    ///     Shortcut for the created test project.
    /// </summary>
    protected Project CurrentProject => CurrentSolution.GetProject(projectId)!;

    /// <summary>
    ///     Setup the workspace and test project.
    /// </summary>
    [SetUp]
    public void SetUpWorkspace()
    {
        workspace = new();
        var project = CreateProject();
        projectId = project.Id;
        workspace.AddProject(project);
    }

    /// <summary>
    ///     Dispose the test workspace.
    /// </summary>
    [TearDown]
    public void TearDownWorkspace()
    {
        workspace.Dispose();
    }

    /// <summary>
    ///     Add a document to <see cref="CurrentProject"/>.
    /// </summary>
    /// <param name="sourceCode">The source code for the new document.</param>
    /// <returns>The created document.</returns>
    /// <remarks>The filename will be randomly generated.</remarks>
    protected Document AddDocument([StringSyntax("C#")] string sourceCode)
    {
        return AddDocument(SourceText.From(sourceCode));
    }

    /// <summary>
    ///     Add a document to <see cref="CurrentProject"/>.
    /// </summary>
    /// <param name="sourceText">The source text for the new document.</param>
    /// <returns>The created document.</returns>
    /// <remarks>The filename will be randomly generated.</remarks>
    protected Document AddDocument(SourceText sourceText)
    {
        return AddDocument(fileName: $"File_{Guid.NewGuid():N}.cs", sourceText: sourceText);
    }

    /// <summary>
    ///     Add a document to <see cref="CurrentProject"/>.
    /// </summary>
    /// <param name="fileName">Filename for the document.</param>
    /// <param name="sourceCode">The source code for the new document.</param>
    /// <returns>The created document.</returns>
    protected Document AddDocument(string fileName, [StringSyntax("C#")] string sourceCode)
    {
        return AddDocument(fileName: fileName, sourceText: SourceText.From(sourceCode));
    }

    /// <summary>
    ///     Add a document to <see cref="CurrentProject"/>.
    /// </summary>
    /// <param name="fileName">Filename for the document.</param>
    /// <param name="sourceText">The source text for the new document.</param>
    /// <returns>The created document.</returns>
    protected Document AddDocument(string fileName, SourceText sourceText)
    {
        var initialDocument = workspace.AddDocument(projectId: projectId, name: fileName, text: sourceText);
        var modifiedSolution = workspace.CurrentSolution.WithDocumentFilePath(documentId: initialDocument.Id, filePath: fileName);
        workspace.TryApplyChanges(modifiedSolution);

        return workspace.CurrentSolution.GetDocument(initialDocument.Id)!;
    }

    /// <summary>
    ///     Return the source code of the given document.
    /// </summary>
    /// <param name="document">Document whose source code to return.</param>
    /// <returns>The raw source code of the document.</returns>
    protected static async Task<string> GetSourceCodeAsync(Document document)
    {
        var rootNode = await document.GetSyntaxRootAsync();

        return rootNode!.ToFullString();
    }

    /// <summary>
    ///     Return the source text of the document with the given id.
    /// </summary>
    /// <param name="documentId">Id of the document whose source text to return.</param>
    /// <returns>The source text of the document.</returns>
    protected async Task<SourceText> GetSourceTextAsync(DocumentId documentId)
    {
        return await CurrentProject.GetDocument(documentId)!.GetTextAsync();
    }

    /// <summary>
    ///     Return the source text of the given document.
    /// </summary>
    /// <param name="document">Document whose source text to return.</param>
    /// <returns>The source text of the document.</returns>
    protected static async Task<SourceText> GetSourceTextAsync(Document document)
    {
        return await document.GetTextAsync();
    }

    /// <summary>
    ///     Return the formatted source code of the given document.
    /// </summary>
    /// <param name="document">Document whose source code to return.</param>
    /// <returns>The formatted source code of the document.</returns>
    /// <remarks>
    ///     The source text is formatted using the <see cref="Formatter"/>.
    /// </remarks>
    protected static async Task<string> GetFormattedSourceCodeAsync(Document document)
    {
        var formattedDocument = await Formatter.FormatAsync(document);

        return await GetSourceCodeAsync(formattedDocument);
    }

    /// <summary>
    ///     Return the formatted source text of the given document.
    /// </summary>
    /// <param name="document">Document whose source text to return.</param>
    /// <returns>The formatted source text of the document.</returns>
    /// <remarks>
    ///     The source text is formatted using the <see cref="Formatter"/>.
    /// </remarks>
    protected static async Task<SourceText> GetFormattedSourceTextAsync(Document document)
    {
        var formattedDocument = await Formatter.FormatAsync(document);

        return await GetSourceTextAsync(formattedDocument);
    }

    /// <summary>
    ///     Create a <see cref="SourceText"/> from a raw string.
    /// </summary>
    /// <param name="text">The code to wrap in a source text.</param>
    /// <returns>The wrapped source text.</returns>
    /// <remarks>
    ///     An IDE will display syntax highlighting for the raw string literal.
    /// </remarks>
    protected static SourceText GetSourceText([StringSyntax("C#")] string text)
    {
        return SourceText.From(text);
    }

    private static ProjectInfo CreateProject()
    {
        return ProjectInfo.Create(
            id: ProjectId.CreateNewId(),
            version: VersionStamp.Create(DateTime.UtcNow),
            name: "TestProject",
            assemblyName: "TestProject",
            language: LanguageNames.CSharp);
    }
}
