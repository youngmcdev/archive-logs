using mcy.Tools.Models;

namespace mcy.Tools.Commands;

public class ArchiveFilesCommand: BaseArchiveCommand
{
    private readonly ArchiveFilesRequest _request;
    public ArchiveFilesCommand(ArchiveFilesRequest request, ArchiveActions archive): base(archive)
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
    protected ArchiveActions _archive;

    public BaseArchiveCommand(ArchiveActions archive)
    {
        _archive = archive;
    }
    public abstract void Execute();
}