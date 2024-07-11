using mcy.CmdTools.Models.Archive;
using mcy.CmdTools.Strategies.Archive;

namespace mcy.CmdTools.Services.Archive;

public interface IArchiveVerifyFileService
{
    void SetStrategy(IArchiveVerifyFileStrategy strategy);
}

public class ArchiveVerifyFileService: IArchiveVerifyFileService
{
    private IArchiveVerifyFileStrategy? _strategy;

    public ArchiveVerifyFileService(){}

    public ArchiveVerifyFileService(IArchiveVerifyFileStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(IArchiveVerifyFileStrategy strategy)
    {
        _strategy = strategy;
    }

    public ArchiveFileToProcess? VerifyFile(FileInfo file) => _strategy?.VerifyFile(file);
}