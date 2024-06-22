namespace mcy.Tools.Models.AppSettings;

public class ArchiveOptions
{
    public const string Archive = "Archive";
    public List<ArchiveLogFileTypeOptions> ArchiveLogFileTypes {get;set;} = new();
    public string PathTo7Zip {get;set;} = String.Empty;
    public int NumberOfDaysToKeepFiles {get;set;}
    public int ArchiveFileNameMonthOffset {get;set;}
}