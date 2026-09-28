namespace Swallow.Refactor.Commands.Refactor;

using Code;
using Execution.Registration;
using Spectre.Console.Cli;
using Symbol;

public sealed class RefactorCommandRegistration : IRegisterableCommand
{
    public static ICommandConfigurator RegisterWith(IConfigurator configurator)
    {
        return configurator.RegisterBranch<RefactorCommandSettings>(
            name: "refactor",
            description: "Apply a series of rewriters to the code",
            commandRegistrations: inner => [RefactorCodeCommand.Register(inner), RefactorSymbolCommand.Register(inner)]);
    }
}
