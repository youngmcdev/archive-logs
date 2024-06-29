using System.CommandLine;
using mcy.CmdTools.Models;

namespace mcy.CmdTools.CliCommands;

public interface IRootCliCommandFactory
{
    RootCommand CreateCommand(CreateRootCliCommandRequest request);
}

public class RootCliCommandFactory: IRootCliCommandFactory
{
    protected RootCommand _rootCommand;
    public RootCommand CreateCommand(CreateRootCliCommandRequest request)
    {
        _rootCommand = new RootCommand(request.Description);
        if(!string.IsNullOrWhiteSpace(request.Alias)) _rootCommand.AddAlias(request.Alias);
        foreach(var command in request.Commands)
        {
            _rootCommand.AddCommand(command);
        }
        foreach(var option in request.Options)
        {
            _rootCommand.AddOption(option);
        }
        return _rootCommand;
    }
}