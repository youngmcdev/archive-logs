using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Commands.Archive;

public class ArchiveFilesCommand: BaseArchiveCommand
{
    private readonly ArchiveFilesRequest _request;
    public ArchiveFilesCommand(ArchiveFilesRequest request, IArchiveActions archive): base(archive)
    {
        _request = request;
    }

    override public void Execute()
    {
        _archive.ArchiveFiles(_request);
    }
}

public abstract class BaseArchiveCommand: ICommand
{
    protected IArchiveActions _archive;

    public BaseArchiveCommand(IArchiveActions archive)
    {
        _archive = archive;
    }
    public abstract void Execute();
}