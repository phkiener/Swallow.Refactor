namespace Swallow.Refactor;

using System.Reflection;
using Commands.Common;
using Execution;
using Execution.Registration;
using Infrastructure;
using Spectre.Console.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        await using var featureCollection = new FeatureCollection();
        var invocation = PreprocessArguments(args);
        var commandApp = BuildCommandApp()
            .RegisterCommonCommands(featureCollection)
            .SetupFeatures(invocation)
            .RegisterCommandsFromAssemblies(invocation.Assemblies, featureCollection);

        await commandApp.RunAsync(invocation.Arguments);
    }

    private static Invocation PreprocessArguments(string[] args)
    {
        var invocation = Preprocessor.Run(args);
        if (invocation.Warnings.Any())
        {
            foreach (var warning in invocation.Warnings)
            {
                Console.WriteLine(" -- " + warning);
            }

            Console.WriteLine();
        }

        return invocation;
    }

    private static ICommandApp BuildCommandApp()
    {
        var app = new CommandApp();
        app.Configure(c => c.SetApplicationName("brrr"));
        app.Configure(c => c.CaseSensitivity(CaseSensitivity.None));
        app.Configure(c => c.PropagateExceptions());
        app.Configure(c => c.ValidateExamples());
        app.Configure(c => c.UseStrictParsing());
        app.Configure(c => c.UseAssemblyInformationalVersion());

        return app;
    }

    private static ICommandApp RegisterCommonCommands(this ICommandApp app, IFeatureCollection featureCollection)
    {
        app.Configure(
            configurator =>
            {
                configurator.AddBranch(
                    name: "rewriter",
                    action: c =>
                    {
                        c.SetDescription("Show registered rewriters");
                        c.Register<ListRewriters>("list", "List all available rewriters").WithData(featureCollection);
                        c.Register<DescribeRewriter>("describe", "Describe the details of a single rewriter").WithData(featureCollection);
                    });

                configurator.AddBranch(
                    name: "symbol-filter",
                    action: c =>
                    {
                        c.SetDescription("Show registered symbol filters");
                        c.Register<ListSymbolFilters>("list", "List all available symbol filters").WithData(featureCollection);
                    });
            });

        return app;
    }

    private static ICommandApp SetupFeatures(this ICommandApp app, Invocation invocation)
    {
        // Iff WorkspaceFeature is after LoggerFeature, loading the solution will be logged, which is what we'd like
        var interceptor = new CompositeInterceptor(
            new AnsiConsoleFeature(),
            new LoggerFeature(),
            new RegistryFeature(invocation.Assemblies),
            new WorkspaceFeature());

        app.Configure(configurator => configurator.SetInterceptor(interceptor));

        return app;
    }


    private static ICommandApp RegisterCommandsFromAssemblies(
        this ICommandApp app,
        IEnumerable<Assembly> assemblies,
        IFeatureCollection featureCollection)
    {
        var registrationMethodStub = typeof(Program).GetMethod(name: nameof(Register), bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
                     ?? throw new MissingMethodException(className: nameof(Program), methodName: nameof(Register));

        var registration = assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsAssignableTo(typeof(IRegisterableCommand)) && t is { IsAbstract: false, IsInterface: false, IsGenericType: false })
            .Select(type => registrationMethodStub.MakeGenericMethod(type));

        foreach (var invokable in registration)
        {
            app.Configure(c => invokable.Invoke(obj: null, parameters: new object?[] { c, featureCollection }));
        }

        return app;
    }

    private static void Register<TCommand>(this IConfigurator configurator, IFeatureCollection featureCollection) where TCommand : IRegisterableCommand
    {
        TCommand.RegisterWith(configurator).WithData(featureCollection);
    }
}
