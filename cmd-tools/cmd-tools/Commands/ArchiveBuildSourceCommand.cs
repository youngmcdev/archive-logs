using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Commands.Archive;

public class ArchiveBuildSourceCommand: BaseArchiveCommand
{
    private readonly BuildArchiveSourceRequest _request;
    public ArchiveBuildSourceCommand(BuildArchiveSourceRequest request, IArchiveActions archive): base(archive)
    {
        _request = request;
    }

    override public void Execute()
    {
        _archive.BuildArchiveSource(_request);
    }
}