namespace mcy.Tools.Models;

public class ArchiveFileSource
{
    public int TotalBytes{get;set;}
    public List<ArchiveFileProperties> Files = new();
}