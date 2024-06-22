using System.CommandLine;

namespace mcy.Tools;

public abstract class BaseCommand
{
    
    public Command BuildCommand()
    {
        return new Command("some command", "command's description");
    }


}