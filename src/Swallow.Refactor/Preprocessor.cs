namespace Swallow.Refactor;

using System.Reflection;
using Core;

public sealed record Invocation(string[] Arguments, Assembly[] Assemblies, string[] Warnings);

public static class Preprocessor
{
    public static Invocation Run(string[] arguments)
    {
        if (arguments is ["-p", _, ..] or ["--plugin", _, ..])
        {
            var additionalAssemblies = ExtraAssemblies(assemblyPaths: arguments[1], separator: ';');

            return new(
                Arguments: arguments.Skip(2).ToArray(),
                Assemblies: GeneralAssemblies().Concat(additionalAssemblies.Assemblies).ToArray(),
                Warnings: additionalAssemblies.Warnings);
        }

        return new(Arguments: arguments, Assemblies: GeneralAssemblies().ToArray(), Warnings: Array.Empty<string>());
    }

    private static IEnumerable<Assembly> GeneralAssemblies()
    {
        yield return typeof(Program).Assembly;
        yield return typeof(Prelude).Assembly;
    }

    private static (Assembly[] Assemblies, string[] Warnings) ExtraAssemblies(string assemblyPaths, char separator)
    {
        var assemblies = new List<Assembly>();
        var warnings = new List<string>();
        foreach (var path in assemblyPaths.Split(
                     separator: separator,
                     options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Path.GetExtension(path) is not ".dll")
            {
                warnings.Add($"Skipping '{path}' because it is not a .dll file.");

                continue;
            }

            if (File.Exists(path) is false)
            {
                warnings.Add($"Skipping '{path}' because it does not exist.");

                continue;
            }

            var absolutePath = Path.GetFullPath(path);
            assemblies.Add(Assembly.LoadFile(absolutePath));
        }

        return (assemblies.ToArray(), warnings.ToArray());
    }
}
