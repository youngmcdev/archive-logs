using mcy.Tools.Models;
using mcy.Tools.Strategies;

namespace mcy.Tools.Services;

public interface IArchiveVerifyFileService
{
    void SetStrategy(IArchiveVerifyFileStrategy strategy);
}

public class ArchiveVerifyFileService: IArchiveVerifyFileService
{
    private IArchiveVerifyFileStrategy _strategy;

    public ArchiveVerifyFileService(){}

    public ArchiveVerifyFileService(IArchiveVerifyFileStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(IArchiveVerifyFileStrategy strategy)
    {
        _strategy = strategy;
    }

    public ArchiveFileProperties? VerifyFile(FileInfo file) => _strategy.VerifyFile(file);
}