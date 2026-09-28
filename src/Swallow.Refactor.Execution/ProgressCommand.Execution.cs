namespace Swallow.Refactor.Execution;

using System.Diagnostics;
using Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

public abstract partial class ProgressCommand<TSettings> where TSettings : CommandSettings
{
    /// <summary>
    ///     Run the given function with a timed progress bar and a final trace message.
    /// </summary>
    /// <param name="name">Name of the task to run, displayed on the progress bar and in the log message.</param>
    /// <param name="func">Function to execute.</param>
    /// <typeparam name="T">Type of the result.</typeparam>
    /// <returns>The result of <paramref name="func"/>.</returns>
    protected T Run<T>(string name, Func<T> func)
    {
        var stopwatch = Stopwatch.StartNew();
        var task = progessContext.AddTask(name);
        var result = func();
        task.StopTask();
        stopwatch.Stop();
        Logger.LogTrace(message: "{task} took {time:g}", name, stopwatch.Elapsed);

        return result;
    }

    /// <summary>
    ///     Run the given function with a timed progress bar and a final trace message.
    /// </summary>
    /// <param name="name">Name of the task to run, displayed on the progress bar and in the log message.</param>
    /// <param name="func">Function to execute.</param>
    protected async Task RunAsync(string name, Func<Task> func)
    {
        var stopwatch = Stopwatch.StartNew();
        var task = progessContext.AddTask(name);
        await func();
        task.StopTask();
        stopwatch.Stop();
        Logger.LogTrace(message: "{task} took {time:g}", name, stopwatch.Elapsed);
    }

    /// <inheritdoc cref="Run{T}"/>
    protected async Task<T> RunAsync<T>(string name, Func<Task<T>> func)
    {
        var stopwatch = Stopwatch.StartNew();
        var task = progessContext.AddTask(name);
        var result = await func();
        task.StopTask();
        stopwatch.Stop();
        Logger.LogTrace(message: "{task} took {time:g}", name, stopwatch.Elapsed);

        return result;
    }

    /// <inheritdoc cref="Run{T}"/>
    protected async Task<IReadOnlyCollection<T>> RunAsync<T>(string name, Func<IAsyncEnumerable<T>> func)
    {
        var elements = new List<T>();
        var stopwatch = Stopwatch.StartNew();
        var task = progessContext.AddTask(name);
        var collection = func();
        await foreach (var elem in collection)
        {
            elements.Add(elem);
        }

        task.StopTask();
        stopwatch.Stop();
        Logger.LogTrace(message: "{task} took {time:g}", name, stopwatch.Elapsed);

        return elements;
    }

    /// <summary>
    ///     Execute <paramref name="func"/> for every element in <paramref name="source"/>, showing a progress bar for the current progress.
    /// </summary>
    /// <param name="name">Function to render a display name for the current element.</param>
    /// <param name="source">Values to process.</param>
    /// <param name="func">The function to use for processing.</param>
    /// <typeparam name="T">Type of the element(s) to process.</typeparam>
    protected async Task ProcessAsync<T>(Func<T, string> name, IEnumerable<T> source, Func<T, Task> func)
    {
        var values = source.ToList();
        if (values.Count == 0)
        {
            return;
        }

        var task = progessContext.AddTask(description: Markup.Escape(name(values[0])), maxValue: values.Count);
        foreach (var value in values)
        {
            task.Description = Markup.Escape(name(value));
            await func(value);
            task.Value += 1;
        }

        task.StopTask();
    }

    /// <summary>
    ///     Execute <paramref name="func"/> as left fold on every element in <paramref name="source"/> using <paramref name="seed"/> as initial value,
    ///     showing a progress bar for the current progress.
    /// </summary>
    /// <param name="name">Function to render a display name for the current element.</param>
    /// <param name="source">Values to process.</param>
    /// <param name="seed">Initial value for the fold.</param>
    /// <param name="func">The function to use for processing.</param>
    /// <typeparam name="T">Type of the element(s) to process.</typeparam>
    /// <typeparam name="U">Type of the initial and produced value.</typeparam>
    /// <seealso cref="ProcessAsync{T}"/>
    /// <seealso cref="Enumerable.Aggregate{TSource}"/>
    protected async Task<U> ProcessAsync<T, U>(Func<T, string> name, IEnumerable<T> source, U seed, Func<T, U, Task<U>> func)
    {
        var accumulate = seed;
        var values = source.ToList();
        if (values.Count == 0)
        {
            return accumulate;
        }

        var task = progessContext.AddTask(description: Markup.Escape(name(values[0])), maxValue: values.Count);
        foreach (var value in values)
        {
            task.Description = Markup.Escape(name(value));
            accumulate = await func(arg1: value, arg2: accumulate);
            task.Value += 1;
        }

        task.StopTask();

        return accumulate;
    }

    /// <summary>
    ///     Execute the changes in an <see cref="IWorkspaceUnitOfWork"/>, printing a progress bar for the current progress.
    /// </summary>
    /// <param name="unitOfWork">The unit of work to execute.</param>
    protected async Task ProcessAsync(IWorkspaceUnitOfWork unitOfWork)
    {
        var filesTask = progessContext.AddTask(description: "Processing files", maxValue: unitOfWork.NumberOfDocuments());
        ProgressTask? currentRewriterTask = null;
        unitOfWork.OnBeginDocument += (_, d) =>
        {
            filesTask.Description = Markup.Escape($"File: {d.FilePath}");
            currentRewriterTask = progessContext.AddTask(description: "Processing rewriters", maxValue: unitOfWork.NumberOfRewriters(d.Id));
        };

        unitOfWork.OnFinishDocument += (_, _) =>
        {
            currentRewriterTask?.StopTask();
            currentRewriterTask = null;
            filesTask.Value += 1;
            if (filesTask.Value >= filesTask.MaxValue)
            {
                filesTask.StopTask();
            }
        };

        unitOfWork.OnBeginRewriter += (_, r) =>
        {
            if (currentRewriterTask is not null)
            {
                currentRewriterTask.Description = $"Rewriter: {r.GetType().Name}";
            }
        };

        unitOfWork.OnFinishRewriter += (_, _) =>
        {
            if (currentRewriterTask is not null)
            {
                currentRewriterTask.Value += 1;
            }
        };

        var stopwatch = Stopwatch.StartNew();
        await unitOfWork.Execute();
        stopwatch.Stop();
        Logger.LogTrace($"Executing UnitOfWork took {stopwatch.Elapsed:g}");
    }
}
