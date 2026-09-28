namespace Swallow.Refactor.Infrastructure.Logging;

using Microsoft.Extensions.Logging;

internal sealed class FileLogger : BasicLoggerBase, IAsyncDisposable
{
    private readonly StreamWriter streamWriter;

    public FileLogger(string path)
    {
        var fileStream = File.Exists(path) ? File.Open(path: path, mode: FileMode.Truncate) : File.Open(path: path, mode: FileMode.CreateNew);
        streamWriter = new(fileStream);
    }

    protected override void WriteMessage(LogLevel logLevel, string message)
    {
        streamWriter.WriteLine(message);
    }

    public async ValueTask DisposeAsync()
    {
        await streamWriter.DisposeAsync();
    }
}
