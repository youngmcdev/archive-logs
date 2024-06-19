using System.CommandLine;

namespace mcy.Tools;

public interface ICommandLineService
{
    RootCommand BuildRootCommand();
}

public class CommandLineService: ICommandLineService
{
    public RootCommand BuildRootCommand()
    {
        return new RootCommand("A program to archive log files.");
    }
}