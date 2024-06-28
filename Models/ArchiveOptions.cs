namespace mcy.Tools.Models.AppSettings;

public class ArchiveOptions
{
    public const string Archive = "Archive";
    public List<ArchiveLogFileTypeOptions> ArchiveLogFileTypes {get;set;} = new();
    public string PathTo7Zip {get;set;} = String.Empty;
    public int NumberOfDaysToKeepFiles {get;set;}
    public int ArchiveFileNameMonthOffset {get;set;}

    public override string ToString()
    {
        return @$"{Archive}
    PathTo7Zip: {PathTo7Zip}
    NumberOfDaysToKeepFiles: {NumberOfDaysToKeepFiles}
    ArchiveFileNameMonthOffset: {ArchiveFileNameMonthOffset}
    Number of Archive Log File Types: {ArchiveLogFileTypes.Count}
{string.Join(Environment.NewLine, ArchiveLogFileTypes.Select(z => z.ToString()))}";
    }
}