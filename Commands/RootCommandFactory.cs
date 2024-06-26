using System.CommandLine;
using mcy.Tools.Models;

namespace mcy.Tools.Commands;

public interface IRootCommandFactory
{
    RootCommand CreateCommand(CreateRootCommandRequest request);
}

public class RootCommandFactory: IRootCommandFactory
{
    protected RootCommand _rootCommand;
    public RootCommand CreateCommand(CreateRootCommandRequest request)
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