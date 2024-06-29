using System.Dynamic;
using System.Text.RegularExpressions;
using mcy.Tools.Models;

namespace mcy.Tools.Strategies;

public interface IArchiveVerifyFileStrategy
{
    abstract ArchiveFileProperties? VerifyFile(FileInfo file);
    public void SetLogger(ILogger? logger);
}

public abstract class ArchiveVerifyFileStrategy: IArchiveVerifyFileStrategy
{
    protected ArchiveVerifyFileRequest _options;
    protected ILogger? _logger;
    public ArchiveVerifyFileStrategy(ArchiveVerifyFileRequest request, ILogger? logger = null)
    {
        _options = request;
        _logger = logger;
    }

    public void SetLogger(ILogger? logger)
    {
        _logger = logger;
    }

    public ArchiveFileProperties? VerifyFile(FileInfo file)
    {
        var dateExtractedFromFileName = GetDateFromFileName(file.Name);
        if(dateExtractedFromFileName is null) return null;

        var fileDate = ParseToDateTime(dateExtractedFromFileName);

        return CompareDates(file, fileDate);
    }

    protected string? GetDateFromFileName(string fileName)
    {
        var fileNamePattern = new Regex(_options.LogFileTypeOptions.FileNamePattern);
        var match = fileNamePattern.Match(fileName);
        if(!match.Success || match.Groups.Count <= _options.LogFileTypeOptions.DatePosition) 
            return null;

        return match.Groups[_options.LogFileTypeOptions.DatePosition].Value;
    }

    protected abstract DateTime ParseToDateTime(string dateString);

    protected ArchiveFileProperties? CompareDates(FileInfo file, DateTime fileDate)
    {
        if(fileDate > _options.ThresholdForArchivingFile)
        {
            Log(string.Format("{0}: The file date, {1}, is greater than {2}. This file will NOT be archived.", 
                file.Name, fileDate.ToShortDateString(), _options.ThresholdForArchivingFile));
            return null;
        }
        else
        {
            Log(string.Format("{0}: The file date, {1}, is less than or equal to {2}. This file will be archived.", 
                file.Name, fileDate.ToShortDateString(), _options.ThresholdForArchivingFile));
        }
        
        return new ArchiveFileProperties
        {
            FileName = file.FullName,
            FileSize = file.Length
        };
    }

    protected void Log(string msg)
    {
        if(_logger is not null) _logger.LogInformation(msg);
    }
}

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

public class ArchiveVerifyFileStrategy_yyyyMM: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyFileStrategy_yyyyMM(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    protected override DateTime ParseToDateTime(string dateString)
    {
        int year = int.Parse(dateString.Substring(0,4)),
            month = int.Parse(dateString.Substring(4,2)),
            day = 27;
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
    }
}

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

public class ArchiveVerifyFileStrategy_yyyy_MM_dd: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyFileStrategy_yyyy_MM_dd(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    protected override DateTime ParseToDateTime(string dateString)
    {
        return DateTime.Parse(dateString);
    }
}