namespace mcy.CmdTools.Infrastructure;

public enum ArchiveLogFileTypes
{
    None = 0,
    
    /// <summary>
    /// Date in Matches[2]. Date in yyMMdd format.
    /// </summary>
    IIS,

    /// <summary>
    /// Date in Matches[2]. Date in yyyyMM format.
    /// </summary>
    SplashPageManager,
    PibItsMe, // Same as SplashPageManager
    GoItsMe, // Same as SplashPageManager

    /// <summary>
    /// Date in Matches[2]. No date parsing required - already in yyyy-MM-dd
    /// </summary>
    ApiMoneyDesktop,

    /// <summary>
    /// Date in Matches[2]. Date in yyyyMMdd format and parsed out into yyyy-MM-DD.
    /// </summary>
    SmsItsMe,
    PibBizLink, // Same as SmsItsMe
    ApiPlatformSettings, // Same as SmsItsMe
    ApiVersions // Same as SmsItsMe

}