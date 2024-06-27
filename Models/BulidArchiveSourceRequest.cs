using mcy.Tools.Infrastructure;

namespace mcy.Tools.Models;

public class BulidArchiveSourceRequest
{
    public string PathToDirectory {get;set;} = String.Empty;
    public ArchiveLogFileTypes LogFileType {get;set;} = ArchiveLogFileTypes.None;
}