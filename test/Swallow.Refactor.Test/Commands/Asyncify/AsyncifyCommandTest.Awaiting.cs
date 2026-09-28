namespace Swallow.Refactor.Commands.Asyncify;

using Testing.Assertion;

internal sealed partial class AsyncifyCommandTest
{
    [Test]
    public async Task MethodWithSynchronousCalls_MethodIsKeptAsIs()
    {
        var file = AddDocument(
            """
            public class Foo
            {
                public void Bar()
                {
                    Console.WriteLine("Stuff");
                }
            }"
            """);

        await RunAsyncifier("void Foo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
            public class Foo
            {
                public async Task BarAsync()
                {
                    Console.WriteLine("Stuff");
                }
            }"
            """);
    }

    [Test]
    public async Task MethodWithAwaitableCall_AwaitsCalls()
    {
        var file = AddDocument(
            """
            public class Foo
            {
                public void Bar()
                {
                    Console.WriteLineAsync("Stuff").GetAwaiter().GetResult();
                }
            }"
            """);

        await RunAsyncifier("void Foo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
            public class Foo
            {
                public async Task BarAsync()
                {
                    await Console.WriteLineAsync("Stuff");
                }
            }"
            """);
    }

    [Test]
    public async Task MethodWithNestedAwaitableCalls_AwaitsAllCalls()
    {
        var file = AddDocument("""
        public class Foo
        {
            public void Bar()
            {
                Console.WriteLineAsync("Size: " + File.GetAllTextAsync("file.txt").GetAwaiter().GetResult().Length).GetAwaiter().GetResult();
            }
        }"
        """);

        await RunAsyncifier("void Foo.Bar()");

        SyntaxAssert.AreEqual(
            actual: await GetSourceTextAsync(file.Id),
            expected: """
            public class Foo
            {
                public async Task BarAsync()
                {
                    await Console.WriteLineAsync("Size: " + (await File.GetAllTextAsync("file.txt")).Length);
                }
            }"
            """);
    }
}
