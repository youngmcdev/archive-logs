using mcy.CmdTools.Infrastructure.Archive;

namespace mcy.CmdTools.Models.Archive;

public class ArchiveDirectoryToProcess
{
    public DirectoryInfo? Directory{get;set;}
    public ArchiveLogFileTypes ArchiveLogFileType{get;set;}
}