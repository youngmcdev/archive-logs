using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public interface IArchiveVerifyFileStrategy_yyyyMMdd : IArchiveVerifyFileStrategy { }

public class ArchiveVerifyFileStrategy_yyyyMMdd: ArchiveVerifyFileStrategy, IArchiveVerifyFileStrategy_yyyyMMdd
{
    public ArchiveVerifyFileStrategy_yyyyMMdd(ILogger<ArchiveVerifyFileStrategy_yyyyMMdd> logger) : base(logger) { }

    protected override DateTime ParseToDateTime(string dateString)
    {
        int year = int.Parse(dateString.Substring(0,4)),
            month = int.Parse(dateString.Substring(4,2)),
            day = int.Parse(dateString.Substring(6,2));
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    }
}