using mcy.CmdTools.Infrastructure;

namespace mcy.CmdTools.Models.Archive;

public class BulidArchiveSourceRequest
{
    public string DirectoryFullPath {get;set;} = String.Empty;
    public ArchiveLogFileTypes LogFileType {get;set;} = ArchiveLogFileTypes.None;
}