using System.CommandLine;
using mcy.Tools.Services;

namespace mcy.Tools;

public interface ICommandExecutor
{
    int Run(string[] commandLineArguments);
}

public class CommandExecutor: ICommandExecutor
{
    private readonly IRootCommandService _commandService;

    public CommandExecutor(IRootCommandService commandService)
    {
        _commandService = commandService;
    }

    public int Run(string[] commandLineArguments)
    {
        return _commandService.BuildRootCommand().Invoke(commandLineArguments);
    }
}