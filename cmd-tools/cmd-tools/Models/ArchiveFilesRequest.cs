namespace mcy.CmdTools.Models.Archive;

public class ArchiveFilesRequest
{
    public bool IsDryRun{get;set;}
    public bool IsDeleteFiles{get;set;}
    public FileInfo? PathTo7Zip {get;set;}
    /// <summary>
    /// [Optional] A file name will be generated if this is not specified.
    /// </summary>
    public FileInfo? ArchiveFileName {get;set;}
    /// <summary>
    /// [Optional] Where the archive will be stored. The current directory will be used if this is not specified.
    /// </summary>
    public DirectoryInfo PathToStoreArchiveFile {get;set;} = new DirectoryInfo("c:\\xx-fake-dir-xx");
}