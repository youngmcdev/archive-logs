using mcy.CmdTools.Models.AppSettings;

namespace mcy.CmdTools.Models.Archive;

public class ArchiveVerifyFileRequest
{
    public DateTime ThresholdForArchivingFile {get;set;} = DateTime.MaxValue;
    public ArchiveLogFileTypeOptions LogFileTypeOptions {get;set;} = new();
}