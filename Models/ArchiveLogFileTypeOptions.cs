using mcy.CmdTools.Infrastructure;

namespace mcy.CmdTools.Models.AppSettings;

public class ArchiveLogFileTypeOptions
{
    public const string ArchiveLogFileType = "ArchiveLogFileType";
    public ArchiveLogFileTypes? LogFileType {get;set;} = ArchiveLogFileTypes.None;
    public string FileNamePattern {get;set;} = "xx-defaplt-pattern-xx";
    public int DatePosition {get;set;}

    public override string ToString()
    {
        return @$"    {ArchiveLogFileType}
        LogFileType: {LogFileType}
        FileNamePattern: {FileNamePattern}
        DatePosition: {DatePosition}";
    }
}