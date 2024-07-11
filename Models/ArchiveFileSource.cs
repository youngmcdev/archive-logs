namespace mcy.CmdTools.Models.Archive;

public class ArchiveFileSource
{
    public long TotalBytes{get;set;}
    public List<ArchiveFileToProcess> Files {get;set; }= new();
}