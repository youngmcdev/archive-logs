using System.Text.RegularExpressions;
using mcy.Tools.Models;

namespace mcy.Tools.Strategies;

public interface IArchiveVerifyFileStrategy
{
    abstract ArchiveFileProperties? VerifyFile(FileInfo file);
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

    public abstract ArchiveFileProperties? VerifyFile(FileInfo file);

    protected void Log(string msg)
    {
        if(_logger is not null) _logger.LogInformation(msg);
    }
}

public class ArchiveVerifyIisFileStrategy: ArchiveVerifyFileStrategy
{
    public ArchiveVerifyIisFileStrategy(ArchiveVerifyFileRequest request, ILogger? logger = null):base(request, logger){}

    public override ArchiveFileProperties? VerifyFile(FileInfo file)
    {
        var fileNamePattern = new Regex(_options.LogFileTypeOptions.FileNamePattern);
        var match = fileNamePattern.Match(file.Name);
        if(!match.Success || match.Groups.Count <= _options.LogFileTypeOptions.DatePosition) return null;

        var fileNameDate = match.Groups[_options.LogFileTypeOptions.DatePosition].Value;
        
        int year = int.Parse($"20{fileNameDate.Substring(0,2)}"),
            month = int.Parse(fileNameDate.Substring(2,2)),
            day = int.Parse(fileNameDate.Substring(4,2));
        var fileDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        
        if(fileDate > _options.ThresholdForArchivingFile)
        {
            Log(string.Format("{0}: {1} is derived from {2} and is greater than {3}. This file will NOT be archived.", 
                file.Name, fileDate.ToShortDateString(), fileNameDate, _options.ThresholdForArchivingFile));
            return null;
        }
        else
        {
            Log(string.Format("{0}: {1} is derived from {2} and is less than or equal to {3}. This file will be archived.", 
                file.Name, fileDate.ToShortDateString(), fileNameDate, _options.ThresholdForArchivingFile));
        }
        
        return new ArchiveFileProperties
        {
            FileName = file.FullName,
            FileSize = file.Length
        };
    }
}