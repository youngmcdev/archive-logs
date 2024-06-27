namespace mcy.Tools.Models;

public class ArchiveFilesRequest
{
    public bool IsDryRun{get;set;}
    public bool IsDeleteFiles{get;set;}
    public FileInfo? PathTo7Zip {get;set;}
}