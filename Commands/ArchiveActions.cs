using System.Diagnostics;
using System.Text.RegularExpressions;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Services;
using mcy.Tools.Strategies;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Commands;
public interface IArchiveActions
{
    ArchiveFileSource ArchiveSource {get;}
    public void BulidArchiveSource(BulidArchiveSourceRequest request);
    public void ArchiveFiles(ArchiveFilesRequest request);
}
public class ArchiveActions: IArchiveActions
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveActions> _logger;
    // a -t7z -m0=lzma -mx=5 -y -sdel {archive-file-name} {list-of-files-to-archive}
    private const string ZipCommandArgsWithDelete = "a -t7z -m0=lzma -mx=5 -y -sdel {0} {1}";
    private const string ZipCommandArgsWithoutDelete = "a -t7z -m0=lzma -mx=5 -y {0} {1}";
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
        ArchiveSource.Files.Clear();
        ArchiveSource.TotalBytes = 0;
        var directory = new DirectoryInfo(request.DirectoryFullPath);
        if(!directory.Exists)
        {
            _logger.LogError("The path, '{0}', could not be found.", request.DirectoryFullPath);
            return;
        }

        var logFileTypeOptions =_config.ArchiveLogFileTypes.Find(z => z.LogFileType == request.LogFileType) ?? new ArchiveLogFileTypeOptions();
        var files = directory.GetFiles();
        ArchiveSource.Files = VerifyFiles(files, logFileTypeOptions).ToList();
        
        _logger.LogInformation(
            "Archiving {0} files in {1} totaling {2} MB.", 
            ArchiveSource.Files.Count,
            directory.FullName,
            Math.Round((double)ArchiveSource.TotalBytes / (1024*1024), 2));
    }

    public void ArchiveFiles(ArchiveFilesRequest request)
    {
        if(request.PathTo7Zip is not {} || !request.PathTo7Zip.Exists)
        {
            _logger.LogError("The path to 7-zip, '{0}', was not found.", request.PathTo7Zip?.FullName ?? string.Empty);
            return;
        }

        try
        {
            var startInfo = GetStartInfo(request);
            using(var process = new Process())
            {
                process.StartInfo = startInfo;
                if(request.IsDryRun)
                {
                    _logger.LogInformation("The --dry-run option was selected. The files will not be archived.");
                    return;
                }
                
                _logger.LogInformation("Begin archiving files in {0}...", startInfo.WorkingDirectory);
                process.Start();
                string stdo = process.StandardOutput.ReadToEnd();
                string stde = process.StandardError.ReadToEnd();
                process.WaitForExit();
                _logger.LogInformation("Finished archiving files in {0}.", startInfo.WorkingDirectory);
                _logger.LogInformation("Logging standard output of the 7zip command:{0}{1}", Environment.NewLine, stdo);
                _logger.LogInformation("Logging standard error of the 7zip command:{0}{1}", Environment.NewLine, stde);
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "An error occurred while archiving files. {0} {1}", e.Message, e.StackTrace);
        }
    }

    private IEnumerable<ArchiveFileProperties> VerifyFiles(FileInfo[] files, ArchiveLogFileTypeOptions logFileTypeOptions)
    {
        var thresholdDate = DateTime.Today.AddDays(_config.NumberOfDaysToKeepFiles);
        var verifyFileRequest = new ArchiveVerifyFileRequest{
            LogFileTypeOptions = logFileTypeOptions,
            ThresholdForArchivingFile = thresholdDate
        };
        // TODO: get strategy based on logFileTypeOptions.LogFileType
        var verifier = new ArchiveVerifyFileService(new ArchiveVerifyIisFileStrategy(verifyFileRequest, _logger)); 
        
        _logger.LogInformation("Threshold Date: {0}; Date Offset: {1}", thresholdDate.ToString(), _config.NumberOfDaysToKeepFiles);
        foreach(var file in files)
        {
            var result = verifier.VerifyFile(file);
            if(result is null) {continue;}

            _logger.LogInformation("    FileName: {0}; Size: {1} MB;", result.FileName, Math.Round((double)result.FileSize /(1024*1024), 2));
            ArchiveSource.TotalBytes += file.Length;
            yield return result;
        }
    }

    private ProcessStartInfo GetStartInfo(ArchiveFilesRequest request)
    {
        var (arguments, workingDirectory) = GetArgumentsAndWorkingDirectory(request);
        _logger.LogInformation("Command to be executed: {0} {1}", request.PathTo7Zip!.FullName, arguments);

        var processStartInfo = new ProcessStartInfo(
            request.PathTo7Zip.FullName,
            arguments
        );

        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.WorkingDirectory = workingDirectory;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.RedirectStandardOutput = true;
        return processStartInfo;
    }

    private (string, string) GetArgumentsAndWorkingDirectory(ArchiveFilesRequest request)
    {
        bool generateFileName = request.ArchiveFileName is null || string.IsNullOrWhiteSpace(request.ArchiveFileName.Name);
        var directoryPath = request.PathToStoreArchiveFile.Exists ? request.PathToStoreArchiveFile : new DirectoryInfo(Directory.GetCurrentDirectory());
        var fileName = generateFileName ? GetArchiveFileName(directoryPath) : request.ArchiveFileName!.Name;

        var args = string.Format(
            request.IsDeleteFiles ? ZipCommandArgsWithDelete : ZipCommandArgsWithoutDelete,
            Path.Combine(directoryPath.FullName, fileName),
            string.Join(' ', ArchiveSource.Files.Select(z => $"\"{z.FileName}\"")
        ));
        return (args, directoryPath.FullName);
    }

    private string GetArchiveFileName(DirectoryInfo directory)
    {
        var archiveFileName = string.Format(Constants.ArchiveFileNameTemplate, 
            directory.Name, 
            DateTime.Today.AddMonths(_config.ArchiveFileNameMonthOffset).ToString("yyyyMM"));
        return archiveFileName;
    }
}