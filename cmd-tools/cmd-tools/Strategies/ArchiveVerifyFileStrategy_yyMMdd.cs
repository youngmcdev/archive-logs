using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public interface IArchiveVerifyFileStrategy_yyMMdd : IArchiveVerifyFileStrategy { }
public class ArchiveVerifyFileStrategy_yyMMdd: ArchiveVerifyFileStrategy, IArchiveVerifyFileStrategy_yyMMdd
{
    public ArchiveVerifyFileStrategy_yyMMdd(ILogger<ArchiveVerifyFileStrategy_yyyyMMdd> logger) :base(logger){ }

    protected override DateTime ParseToDateTime(string dateString)
    {
        int year = int.Parse($"20{dateString.Substring(0,2)}"),
            month = int.Parse(dateString.Substring(2,2)),
            day = int.Parse(dateString.Substring(4,2));
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    }
}