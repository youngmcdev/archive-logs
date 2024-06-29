using mcy.Tools.Models.AppSettings;

namespace mcy.Tools.Models;

public class ArchiveVerifyFileRequest
{
    public DateTime ThresholdForArchivingFile {get;set;} = DateTime.MaxValue;
    public ArchiveLogFileTypeOptions LogFileTypeOptions {get;set;} = new();
}