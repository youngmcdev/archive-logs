namespace mcy.CmdTools.Models.AppSettings;

public class ArchiveOptions
{
    public const string Archive = "Archive";
    public List<ArchiveLogFileTypeOptions> ArchiveLogFileTypes {get;set;} = new();
    public string PathTo7Zip {get;set;} = String.Empty;
    public int NumberOfDaysToKeepFiles {get;set;}
    public int ArchiveFileNameMonthOffset {get;set;}
    public List<ArchiveCommandToInvoke> ArchiveCommandsToInvoke {get;set;} = new();

    public override string ToString()
    {
        return @$"{Archive}
    PathTo7Zip: {PathTo7Zip}
    NumberOfDaysToKeepFiles: {NumberOfDaysToKeepFiles}
    ArchiveFileNameMonthOffset: {ArchiveFileNameMonthOffset}
Number of Archive Log File Types: {ArchiveLogFileTypes.Count}
{string.Join(Environment.NewLine, ArchiveLogFileTypes.Select(z => z.ToString()))}
Number of Archive Commands to Invoke: {ArchiveCommandsToInvoke.Count}
{string.Join(Environment.NewLine, ArchiveCommandsToInvoke.Select(z => z.ToString()))}";
    }
}