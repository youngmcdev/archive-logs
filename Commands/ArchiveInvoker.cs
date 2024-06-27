namespace mcy.Tools.Commands;

public class ArchiveInvoker
{
    private ICommand? _command;

    public void SetCommand(ICommand command)
    {
        _command = command;
    }

    public void ExecuteCommand()
    {
        _command?.Execute();
    }
}