namespace mcy.CmdTools.Models.Archive;

public class ArchiveFileSource
{
    public long TotalBytes{get;set;}
    public List<ArchiveFileProperties> Files {get;set; }= new();
}