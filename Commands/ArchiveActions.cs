using System.Text.RegularExpressions;
using mcy.Tools.Models;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Services;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Commands;

public class ArchiveActions
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveActions> _logger;
    public ArchiveFileSource ArchiveSource {get;protected set;} = new();

    public ArchiveActions(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveActions> logger
    )
    {
        _config = config.Value;
        _logger = logger;
    }

    public void BulidArchiveSource(BulidArchiveSourceRequest request)
    {
        var directory = new DirectoryInfo(request.PathToDirectory);
        if(!directory.Exists) return;

        var logFileTypeOptions =_config.ArchiveLogFileTypes.Find(z => z.LogFileType == request.LogFileType);
        var fileNamePattern = new Regex(logFileTypeOptions?.FileNamePattern ?? string.Empty);
        var files = directory.GetFiles();
        ArchiveSource.TotalBytes = 0;
        ArchiveSource.Files = MapFiles().ToList();
        

        IEnumerable<ArchiveFileProperties> MapFiles()
        {
            foreach(var file in files)
            {
                var filename = file.Name;
                if(!fileNamePattern.IsMatch(filename)) continue;
                // TODO: Check that the date in the file name is within the cutoff range.

                ArchiveSource.TotalBytes += file.Length;
                yield return new ArchiveFileProperties
                { 
                    FileName = file.FullName,
                    FileSize = file.Length
                };
            }
        }
    }

    public void ArchiveFiles(ArchiveFilesRequest request)
    {

    }
}