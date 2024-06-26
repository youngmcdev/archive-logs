using System.CommandLine;

namespace mcy.Tools.CliCommands;

public interface ICliCommandFactory
{
    abstract Command CreateCommand();
}
public abstract class CliCommandFactory: ICliCommandFactory
{
    protected Command _command;

    public abstract Command CreateCommand();
}
