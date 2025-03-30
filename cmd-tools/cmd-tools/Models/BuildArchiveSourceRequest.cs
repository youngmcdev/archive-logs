using mcy.CmdTools.Infrastructure.Archive;

namespace mcy.CmdTools.Models.Archive;

public class BuildArchiveSourceRequest
{
    public string DirectoryFullPath {get;set;} = String.Empty;
    public ArchiveLogFileTypes LogFileType {get;set;} = ArchiveLogFileTypes.None;
}