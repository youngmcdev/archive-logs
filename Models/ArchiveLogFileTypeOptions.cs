using mcy.Tools.Infrastructure;

namespace mcy.Tools.Models.AppSettings;

public class ArchiveLogFileTypeOptions
{
    public const string ArchiveLogFileType = "ArchiveLogFileType";
    public ArchiveLogFileTypes LogFileType {get;set;} = ArchiveLogFileTypes.None;
    public string FileNamePattern {get;set;} = String.Empty;
    public int DatePosition {get;set;}
}