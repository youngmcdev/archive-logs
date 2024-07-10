using mcy.CmdTools.Infrastructure;

namespace mcy.CmdTools.Models.AppSettings;

public class ArchiveCommandToInvoke
{
    public const string ArchiveCommandsToInvoke = "ArchiveCommandsToInvoke";
    public List<string> Directories{get;set;} = new();
    public ArchiveLogFileTypes? LogFileType {get;set;} = ArchiveLogFileTypes.None;
    public override string ToString()
    {
        return $@"    {ArchiveCommandsToInvoke}
        LogFileType: {LogFileType}
        Directories: {string.Join(", ", Directories)}";
    }
}