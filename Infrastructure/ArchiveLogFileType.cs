namespace mcy.Tools.Infrastructure;

[Serializable]
public enum ArchiveLogFileTypes
{
    None = 0,
    /// <summary>
    /// Date in Matches[2]. Date in yyMMDD format.
    /// </summary>
    IIS = 1,
    /// <summary>
    /// Date in Matches[2]. Date in yyyyMM format.
    /// </summary>
    SplashPageManager = 2,
    /// <summary>
    /// Date in Matches[2]. No date parsing required - already in yyyy-MM-DD
    /// </summary>
    ApiMoneyDesktop = 3,
    /// <summary>
    /// Date in Matches[2]. Date in yyyyMMDD format and parsed out into yyyy-MM-DD.
    /// </summary>
    SmsItsMe = 4
}