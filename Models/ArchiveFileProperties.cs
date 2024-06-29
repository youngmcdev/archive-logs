namespace mcy.CmdTools.Models.Archive;

public class ArchiveFileProperties
{
    public string FileName{get;set;} = String.Empty;
    /// <summary>
    /// Number of bytes
    /// </summary>
    public long FileSize{get;set;}
}