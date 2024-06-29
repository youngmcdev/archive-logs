using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public class ArchiveVerifyFileStrategy_yyMMdd: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyFileStrategy_yyMMdd(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    protected override DateTime ParseToDateTime(string dateString)
    {
        int year = int.Parse($"20{dateString.Substring(0,2)}"),
            month = int.Parse(dateString.Substring(2,2)),
            day = int.Parse(dateString.Substring(4,2));
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    }
}