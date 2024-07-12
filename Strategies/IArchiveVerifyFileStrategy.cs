using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public interface IArchiveVerifyFileStrategy
{
    abstract ArchiveFileToProcess? VerifyFile(FileInfo file);
    public ArchiveVerifyFileStrategy SetLogger(ILogger? logger);
}