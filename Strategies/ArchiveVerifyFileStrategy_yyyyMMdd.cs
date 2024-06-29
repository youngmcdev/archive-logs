using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public class ArchiveVerifyFileStrategy_yyyyMMdd: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyFileStrategy_yyyyMMdd(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    protected override DateTime ParseToDateTime(string dateString)
    {
        int year = int.Parse(dateString.Substring(0,4)),
            month = int.Parse(dateString.Substring(4,2)),
            day = int.Parse(dateString.Substring(6,2));
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    }
}