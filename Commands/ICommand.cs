using mcy.Tools.Models;

namespace mcy.Tools.Commands;

public interface ICommand
{
    void Execute();
}

public interface IExternalCommand: ICommand{}

public abstract class ExternalCommand: IExternalCommand
{

    public ExternalCommand()
    {

    }
    public void Execute()
    {

    }
}

public class ExternalCommandRequest
{

}