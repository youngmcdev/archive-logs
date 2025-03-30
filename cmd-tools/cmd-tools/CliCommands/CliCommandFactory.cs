using System.CommandLine;

namespace mcy.CmdTools.CliCommands;

public interface ICliCommandFactory
{
    abstract Command CreateCommand();
}
public abstract class CliCommandFactory: ICliCommandFactory
{
    protected Command? _command;

    public abstract Command CreateCommand();
}
