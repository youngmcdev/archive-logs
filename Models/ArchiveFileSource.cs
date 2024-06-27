namespace mcy.Tools.Models;

public class ArchiveFileSource
{
    public long TotalBytes{get;set;}
    public List<ArchiveFileProperties> Files = new();
}