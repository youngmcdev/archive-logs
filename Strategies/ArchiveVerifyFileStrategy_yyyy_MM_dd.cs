using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public class ArchiveVerifyFileStrategy_yyyy_MM_dd: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyFileStrategy_yyyy_MM_dd(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    protected override DateTime ParseToDateTime(string dateString)
    {
        return DateTime.Parse(dateString);
    }
}