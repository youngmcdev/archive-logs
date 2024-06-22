using System.CommandLine;

namespace mcy.Tools;

public interface ICommandExecutor
{
    int Run();
}

public class CommandExecutor: ICommandExecutor
{
    private readonly string[] _commandlineArguments;
    
    public CommandExecutor(string[] args)
    {
        _commandlineArguments = args;
    }

    public int Run()
    {
        return Execute(new ArchiveRootCommandService());
    }

    public int Execute(IRootCommandService commandService)
    {
        return commandService.BuildRootCommand().Invoke(_commandlineArguments);
    }
}