using System.Text.RegularExpressions;
using mcy.CmdTools.Commands.Archive;
using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Strategies.Archive;

public abstract class ArchiveVerifyFileStrategy: IArchiveVerifyFileStrategy
{
    protected ArchiveVerifyFileRequest _options;
    protected ILogger<ArchiveVerifyFileStrategy>? _logger;
    public ArchiveVerifyFileStrategy(ILogger<ArchiveVerifyFileStrategy> logger)
    {
        _logger = logger;
    }

    public IArchiveVerifyFileStrategy SetStrategyOptions( ArchiveVerifyFileRequest options)
    {
        _options = options;
        return this;
    }

    public ArchiveFileToProcess? VerifyFile(FileInfo file)
    {
        var dateExtractedFromFileName = GetDateFromFileName(file.Name);
        if(dateExtractedFromFileName is null) return null;

        var fileDate = ParseToDateTime(dateExtractedFromFileName);

        return CompareDates(file, fileDate);
    }

    private string? GetDateFromFileName(string fileName)
    {
        var fileNamePattern = new Regex(_options.LogFileTypeOptions.FileNamePattern);
        var match = fileNamePattern.Match(fileName);
        if(!match.Success || match.Groups.Count <= _options.LogFileTypeOptions.DatePosition) 
            return null;

        return match.Groups[_options.LogFileTypeOptions.DatePosition].Value;
    }

    protected abstract DateTime ParseToDateTime(string dateString);

    private ArchiveFileToProcess? CompareDates(FileInfo file, DateTime fileDate)
    {

        if(fileDate > _options.ThresholdForArchivingFile)
        {
            Log(string.Format("    {0}: The file date, {1}, is greater than {2}. This file will NOT be archived.", 
                file.Name, fileDate.ToShortDateString(), _options.ThresholdForArchivingFile));
            return null;
        }
        else
        {
            Log(string.Format("    {0}: The file date, {1}, is less than or equal to {2}. This file will be archived.", 
                file.Name, fileDate.ToShortDateString(), _options.ThresholdForArchivingFile));
        }
        
        return new ArchiveFileToProcess
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
