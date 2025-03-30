using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public interface IArchiveVerifyFileStrategy_yyyy_MM_dd : IArchiveVerifyFileStrategy { }

public class ArchiveVerifyFileStrategy_yyyy_MM_dd: ArchiveVerifyFileStrategy, IArchiveVerifyFileStrategy_yyyy_MM_dd
{
    public ArchiveVerifyFileStrategy_yyyy_MM_dd(ILogger<ArchiveVerifyFileStrategy_yyyyMMdd> logger) : base(logger) { }

    protected override DateTime ParseToDateTime(string dateString)
    {
        return DateTime.Parse(dateString);
    }
}