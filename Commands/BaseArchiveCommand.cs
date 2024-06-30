namespace mcy.CmdTools.Commands.Archive;

public abstract class BaseArchiveCommand: ICommand
{
    protected IArchiveActions _archive;

    public BaseArchiveCommand(IArchiveActions archive)
    {
        _archive = archive;
    }
    public abstract void Execute();
}