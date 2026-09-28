namespace Swallow.Refactor.Infrastructure;

using Spectre.Console.Cli;

internal sealed class CompositeInterceptor : ICommandInterceptor
{
    private readonly IEnumerable<ICommandInterceptor> interceptors;

    public CompositeInterceptor(params ICommandInterceptor[] interceptors)
    {
        this.interceptors = interceptors;
    }

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        foreach (var interceptor in interceptors)
        {
            interceptor.Intercept(context, settings);
        }
    }
}
