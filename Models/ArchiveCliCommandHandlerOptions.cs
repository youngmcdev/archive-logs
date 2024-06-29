using mcy.CmdTools.Infrastructure;

namespace mcy.CmdTools.Models.Archive;

public class ArchiveCliCommandHandlerOptions
{
    public bool IsDryRun{get;set;}
    public bool IsDeleteFiles{get;set;}
    public bool IsDirectoriesFromConfigurationFile{get;set;}
    public List<DirectoryInfo> Directories{get;set;} = new List<DirectoryInfo>();
    public ArchiveLogFileTypes ArchiveLogFileType{get;set;} = ArchiveLogFileTypes.None;
    public FileInfo? PathTo7Zip {get;set;}

    public override string ToString()
    {
        return @$"
    IsDryRun: {IsDryRun}
    IsDeleteFiles: {IsDeleteFiles}
    IsDirectoriesFromConfigurationFile: {IsDirectoriesFromConfigurationFile}
    ArchiveLogFileTypes: {ArchiveLogFileType}
    PathTo7Zip: {PathTo7Zip?.FullName ?? "Not Specified"}
    Directories: {string.Join(", ", Directories?.Select(d => d.FullName) ?? Array.Empty<string>()) }
";
    }
}