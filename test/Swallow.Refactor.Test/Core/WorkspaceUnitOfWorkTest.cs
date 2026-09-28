namespace Swallow.Refactor.Core;

using Modify;
using Abstractions.Rewriting;
using Testing;
using Testing.Assertion;

internal sealed class WorkspaceUnitOfWorkTest : RoslynTest
{
    [Test]
    public async Task NoChangesStaged_DoesNothing()
    {
        var previousSolution = CurrentSolution;
        var unitOfWork = Workspace.BeginChanges();
        await unitOfWork.Execute();
        Assert.That(actual: CurrentSolution.Version, expression: Is.EqualTo(previousSolution.Version));
    }

    [Test]
    public async Task ThrowsException_WhenChangesAreExecutedTwice()
    {
        var unitOfWork = Workspace.BeginChanges();
        await unitOfWork.Execute();
        Assert.That(
            del: async () => await unitOfWork.Execute(),
            expr: Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("Changes have already been executed."));
    }

    [Test]
    public async Task ExecutesRewritersForSingleDocument()
    {
        var document = AddDocument(
            """
using System;

namespace Foo { public sealed record Bar; }
""");

        var unitOfWork = Workspace.BeginChanges();
        unitOfWork.RecordChange(documentId: document.Id, documentRewriter: new TestDocumentRewriter("System.IO"));
        unitOfWork.RecordChange(documentId: document.Id, documentRewriter: new TestDocumentRewriter("System.Threading.Tasks"));
        await unitOfWork.Execute();
        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(document.Id)!.GetTextAsync(),
            expected: """
using System;
using System.IO;
using System.Threading.Tasks;

namespace Foo { public sealed record Bar; }
""");
    }

    [Test]
    public async Task ExecutesRewritersForMultipleDocuments()
    {
        var firstDocument = AddDocument(
            """
using System;

namespace Foo { public sealed record Bar; }
""");

        var secondDocument = AddDocument(
            """
using System;

namespace Bar { public sealed record Foo; }
""");

        var unitOfWork = Workspace.BeginChanges();
        unitOfWork.RecordChange(documentId: firstDocument.Id, documentRewriter: new TestDocumentRewriter("System.IO"));
        unitOfWork.RecordChange(documentId: secondDocument.Id, documentRewriter: new TestDocumentRewriter("System.Threading.Tasks"));
        await unitOfWork.Execute();
        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(firstDocument.Id)!.GetTextAsync(),
            expected: """
using System;
using System.IO;

namespace Foo { public sealed record Bar; }
""");

        SyntaxAssert.AreEqual(
            actual: await CurrentSolution.GetDocument(secondDocument.Id)!.GetTextAsync(),
            expected: """
using System;
using System.Threading.Tasks;

namespace Bar { public sealed record Foo; }
""");
    }

    [Test]
    public async Task EventsAreTriggeredInCorrectOrder()
    {
        var firstDocument = AddDocument("namespace Foo { public sealed record Bar; }");
        var secondDocument = AddDocument("namespace Bar { public sealed record Foo; }");
        var unitOfWork = Workspace.BeginChanges();
        unitOfWork.RecordChange(documentId: firstDocument.Id, documentRewriter: new TestDocumentRewriter("System.IO"));
        unitOfWork.RecordChange(documentId: secondDocument.Id, documentRewriter: new TestDocumentRewriter("System.Threading.Tasks"));
        var events = new List<string>();
        unitOfWork.OnBeginDocument += (_, d) => events.Add($"{nameof(unitOfWork.OnBeginDocument)}: {d.Id}");
        unitOfWork.OnFinishDocument += (_, d) => events.Add($"{nameof(unitOfWork.OnFinishDocument)}: {d.Id}");
        unitOfWork.OnBeginRewriter += (_, r) => events.Add($"{nameof(unitOfWork.OnBeginRewriter)}: {r.GetType().Name}");
        unitOfWork.OnFinishRewriter += (_, r) => events.Add($"{nameof(unitOfWork.OnFinishRewriter)}: {r.GetType().Name}");
        await unitOfWork.Execute();
        var expectedEvents = new[]
        {
            $"{nameof(unitOfWork.OnBeginDocument)}: {firstDocument.Id}",
            $"{nameof(unitOfWork.OnBeginRewriter)}: {nameof(TestDocumentRewriter)}",
            $"{nameof(unitOfWork.OnFinishRewriter)}: {nameof(TestDocumentRewriter)}",
            $"{nameof(unitOfWork.OnFinishDocument)}: {firstDocument.Id}",
            $"{nameof(unitOfWork.OnBeginDocument)}: {secondDocument.Id}",
            $"{nameof(unitOfWork.OnBeginRewriter)}: {nameof(TestDocumentRewriter)}",
            $"{nameof(unitOfWork.OnFinishRewriter)}: {nameof(TestDocumentRewriter)}",
            $"{nameof(unitOfWork.OnFinishDocument)}: {secondDocument.Id}"
        };

        Assert.That(actual: events, expression: Is.EqualTo(expectedEvents));
    }

    private sealed class TestDocumentRewriter : IDocumentRewriter
    {
        private readonly string namespaceToAdd;

        public TestDocumentRewriter(string namespaceToAdd)
        {
            this.namespaceToAdd = namespaceToAdd;
        }

        public Task RunAsync(DocumentEditor documentEditor, SyntaxTree syntaxTree)
        {
            var compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var changedCompilationUnit = compilationUnit.AddUsing(namespaceToAdd);
            documentEditor.ReplaceNode(node: compilationUnit, newNode: changedCompilationUnit);

            return Task.CompletedTask;
        }
    }
}
