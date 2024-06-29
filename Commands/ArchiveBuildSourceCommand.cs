using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Commands.Archive;

public class ArchiveBuildSourceCommand: BaseArchiveCommand
{
    private readonly BulidArchiveSourceRequest _request;
    public ArchiveBuildSourceCommand(BulidArchiveSourceRequest request, IArchiveActions archive): base(archive)
    {
        _request = request;
    }

    override public void Execute()
    {
        _archive.BulidArchiveSource(_request);
    }
}