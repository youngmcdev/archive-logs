using System.Diagnostics;
using mcy.CmdTools.Infrastructure;
using mcy.CmdTools.Models.Archive;
using mcy.CmdTools.Models.AppSettings;
using mcy.CmdTools.Services.Archive;
using mcy.CmdTools.Strategies.Archive;
using Microsoft.Extensions.Options;
using System.Text;
using mcy.CmdTools.Infrastructure.Archive;

namespace mcy.CmdTools.Commands.Archive;
public interface IArchiveActions
{
    ArchiveFileSource ArchiveSource {get;}
    public void BuildArchiveSource(BuildArchiveSourceRequest request);
    public void ArchiveFiles(ArchiveFilesRequest request);
}

public class ArchiveActions: IArchiveActions
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveActions> _logger;
    private readonly IServiceProvider _serviceProvider;
    // a -t7z -m0=lzma -mx=5 -y -sdel {archive-file-name} {list-of-files-to-archive}
    private const string ZipCommandArgsWithDelete = "a -t7z -m0=lzma -mx=5 -y -sdel \"{0}\" {1}";
    private const string ZipCommandArgsWithoutDelete = "a -t7z -m0=lzma -mx=5 -y \"{0}\" {1}";
    public ArchiveFileSource ArchiveSource {get;protected set;} = new();

    public ArchiveActions(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveActions> logger,
        IServiceProvider serviceProvider
    )
    {
        _config = config.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void BuildArchiveSource(BuildArchiveSourceRequest request)
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
            "    Archiving {0} files in {1} totaling {2} MB.", 
            ArchiveSource.Files.Count,
            directory.FullName,
            Math.Round((double)ArchiveSource.TotalBytes / (1024*1024), 2));
    }

    public void ArchiveFiles(ArchiveFilesRequest request)
    {
        var endl = Environment.NewLine;
        var sb = new StringBuilder();
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
                    sb.Append("        The --dry-run option was selected. The files will not be archived.");
                    return;
                }
                
                sb.AppendFormat("    {0}Begin archiving files in {1}...", sb.Length > 0 ? endl : string.Empty, startInfo.WorkingDirectory);
                process.Start();
                string stdo = process.StandardOutput.ReadToEnd();
                string stde = process.StandardError.ReadToEnd();
                process.WaitForExit();
                sb.AppendFormat("    {0}Finished archiving files in {1}.", endl, startInfo.WorkingDirectory);
                sb.AppendFormat("    {0}Logging standard output of the 7zip command:{1}{2}", endl, endl, stdo);
                sb.AppendFormat("    {0}Logging standard error of the 7zip command:{1}{2}{3}", endl, endl, stde.Length > 0 ? stde : "No standard error output.", endl);
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "An error occurred while archiving files. {0} {1}", e.Message, e.StackTrace);
        }
        finally
        {
            _logger.LogInformation(sb.ToString());
        }
    }

    private IEnumerable<ArchiveFileToProcess> VerifyFiles(FileInfo[] files, ArchiveLogFileTypeOptions logFileTypeOptions)
    {
        var thresholdDate = DateTime.Today.AddDays(_config.NumberOfDaysToKeepFiles);
        var verifyFileRequest = new ArchiveVerifyFileRequest{
            LogFileTypeOptions = logFileTypeOptions,
            ThresholdForArchivingFile = thresholdDate
        };

        var strategy = GetVerifyFileStrategy(verifyFileRequest.LogFileTypeOptions.LogFileType).SetStrategyOptions(verifyFileRequest);
        var verifier = new ArchiveVerifyFileService(strategy);
        
        _logger.LogInformation("    Verifying files - Threshold Date: {0}; Date Offset: {1}", thresholdDate.ToString(), _config.NumberOfDaysToKeepFiles);
        foreach(var file in files)
        {
            var result = verifier.VerifyFile(file);
            if(result is null) {continue;}

            _logger.LogInformation("        FileName: {1}; Size: {2} MB;", result.FileName, Math.Round((double)result.FileSize /(1024*1024), 2));
            ArchiveSource.TotalBytes += file.Length;
            yield return result;
        }
    }

    private IArchiveVerifyFileStrategy GetVerifyFileStrategy(ArchiveLogFileTypes? logFiletype)
    {
        if (!logFiletype.HasValue) logFiletype = ArchiveLogFileTypes.None;;
        IArchiveVerifyFileStrategy strategy = logFiletype switch
        {
            ArchiveLogFileTypes.IIS => _serviceProvider.GetRequiredService<IArchiveVerifyFileStrategy_yyMMdd>(),
            ArchiveLogFileTypes.SmsItsMe or ArchiveLogFileTypes.PibBizLink or ArchiveLogFileTypes.ApiPlatformSettings or ArchiveLogFileTypes.ApiVersions => _serviceProvider.GetRequiredService<IArchiveVerifyFileStrategy_yyyyMMdd>(),
            ArchiveLogFileTypes.SplashPageManager or ArchiveLogFileTypes.PibItsMe or ArchiveLogFileTypes.GoItsMe => _serviceProvider.GetRequiredService<IArchiveVerifyFileStrategy_yyyyMM>(),
            _ => _serviceProvider.GetRequiredService<IArchiveVerifyFileStrategy_yyyy_MM_dd>()
        };

        return strategy;
    }

    private ProcessStartInfo GetStartInfo(ArchiveFilesRequest request)
    {
        var (arguments, workingDirectory) = GetArgumentsAndWorkingDirectory(request);
        _logger.LogInformation("    Preparing 7zip command to be executed: {0} {1}", request.PathTo7Zip!.FullName, arguments);

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