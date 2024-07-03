using System.CommandLine;
using mcy.CmdTools.CliOptions;
using mcy.CmdTools.Infrastructure.Archive;
using mcy.CmdTools.Infrastructure;
using mcy.CmdTools.Models.AppSettings;
using mcy.CmdTools.Models;
using Microsoft.Extensions.Options;
using mcy.CmdTools.Commands.Archive;
using System.CommandLine.Parsing;
using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.CliCommands;

public interface IArchiveCliCommandFactory : ICliCommandFactory { }
public class ArchiveCliCommandFactory : CliCommandFactory, IArchiveCliCommandFactory
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveCliCommandFactory> _logger;
    private readonly IBoolCliOptionFactory _boolOptionFactory;
    private readonly ICliOptionFactory<FileInfo> _fileOptionFactory;
    private readonly ICliOptionFactory<IEnumerable<DirectoryInfo>> _directoriesOptionFactory;
    private readonly ICliOptionFactory<ArchiveLogFileTypes> _fileTypeOptionFactory;
    private readonly IArchiveActions _archiveActions;
    public ArchiveCliCommandFactory(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveCliCommandFactory> logger,
        IBoolCliOptionFactory boolOptionFactory, // TODO: Perhaps an abstract factory would help reduce the number of dependencies being injected.
        ICliOptionFactory<FileInfo> fileOptionFactory,
        ICliOptionFactory<IEnumerable<DirectoryInfo>> filesOptionFactory,
        ICliOptionFactory<ArchiveLogFileTypes> fileTypeOptionFactory,
        IArchiveActions archiveActions
    )
    {
        _config = config.Value;
        _logger = logger;
        _boolOptionFactory = boolOptionFactory;
        _fileOptionFactory = fileOptionFactory;
        _directoriesOptionFactory = filesOptionFactory;
        _fileTypeOptionFactory = fileTypeOptionFactory;
        _archiveActions = archiveActions;
    }

    public override Command CreateCommand()
    {
        var dryRunOption = _boolOptionFactory.CreateOption(new CreateBoolCliOptionRequest
        {
            Name = "--dry-run",
            Description = "If true, the archiving will to occur. However, the directories and files will be iterated over, and logging will be written. Defaults to false."
        });

        var deleteFilesOption = _boolOptionFactory.CreateOption(new CreateBoolCliOptionRequest
        {
            Name = "--delete-files",
            Alias = "-del",
            Description = "If true, the original, archived files will be deleted. Defaults to false."
        });

        var directoriesFromConfigurationFile = _boolOptionFactory.CreateOption(new CreateBoolCliOptionRequest
        {
            Name = "--directories-from-config",
            Description = "If true, then --directories is ignored. Multiple sets of directories and their respective --log-file-type values may be specified in appsettings.json. This is a way to queue multiple runs of the archive command. Defaults to false."
        });

        var directoriesOption = _directoriesOptionFactory.CreateOption(new CreateCliOptionRequest<IEnumerable<DirectoryInfo>>
        {
            Name = "--directories",
            Alias = "-d",
            Description = "A list of directories to search for log files. An archive file will be left in each directory where log files are found.",
            IsRequired = true,
            IsDefault = true,
            AllowMultipleArgumentsPerToken = true,
            ParseArgumentDelegate = ParseDirectoriesDelegate
        });

        var pathTo7zipOption = _fileOptionFactory.CreateOption(new CreateCliOptionRequest<FileInfo>
        {
            Name = "--path-to-zip",
            Alias = "-7z",
            Description = "Path to the 7-zip program. Overrides the value in appSettings.json.",
            IsDefault = true,
            ParseArgumentDelegate = ParsePathToZipDelegate
        });

        var logFileTypeOption = _fileTypeOptionFactory.CreateOption(new CreateCliOptionRequest<ArchiveLogFileTypes>
        {
            Name = "--log-file-type",
            Alias = "-t",
            Description = "The type of log file that will be looked for in the directories specified. Other file will be ignored. A file type uses a regular expression to compare to file names and expects the date to be in the file name at a specific, relative location in the file name.",
            IsRequired = true
        });

        _command = new Command("archive", "Archive log files and optionally delete the original files.")
        {
            dryRunOption,
            deleteFilesOption,
            directoriesOption,
            logFileTypeOption,
            pathTo7zipOption,
            directoriesFromConfigurationFile
        };

        _command.AddAlias("ark");

        _command.SetHandler(
            CommandHandlerDelegate, 
            new ArchiveCliCommandHandlerOptionsBinder(dryRunOption, deleteFilesOption, directoriesFromConfigurationFile, directoriesOption, logFileTypeOption, pathTo7zipOption));

        return _command;
    }

    // TODO: Need a way to queue multiple runs of the archive command from appsettings.json.

    // TODO: Possibly move these delegates to services that are injected.
    private void CommandHandlerDelegate(ArchiveCliCommandHandlerOptions archiveCommandHandlerOptions)
    {
        int totalFilesArchived = 0;
        long totalBytesArchived = 0;
        _logger.LogInformation("Begin the file archiving process - {0}", DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss zzz"));
        _logger.LogDebug("Archive Command Options: {0}", archiveCommandHandlerOptions.ToString());

        foreach(var currDirectory in archiveCommandHandlerOptions.Directories) {ProcessDirectory(currDirectory);}

        void ProcessDirectory(DirectoryInfo dirInfo)
        {
            _logger.LogInformation("Processing files in {0}", dirInfo.FullName);
            var archiveInvoker = new ArchiveInvoker();
            archiveInvoker.SetCommand(new ArchiveBuildSourceCommand(
                new BuildArchiveSourceRequest{
                    LogFileType = archiveCommandHandlerOptions.ArchiveLogFileType,
                    DirectoryFullPath = dirInfo.FullName
                }, 
                _archiveActions));
            archiveInvoker.ExecuteCommand();
            
            if(_archiveActions.ArchiveSource.Files.Count < 1) return;
            
            totalFilesArchived += _archiveActions.ArchiveSource.Files.Count;
            totalBytesArchived += _archiveActions.ArchiveSource.TotalBytes;
            Directory.SetCurrentDirectory(dirInfo.FullName);

            archiveInvoker.SetCommand(new ArchiveFilesCommand(
                new ArchiveFilesRequest{
                    IsDryRun = archiveCommandHandlerOptions.IsDryRun,
                    IsDeleteFiles = archiveCommandHandlerOptions.IsDeleteFiles,
                    PathTo7Zip = archiveCommandHandlerOptions.PathTo7Zip
                }, 
                _archiveActions));
            archiveInvoker.ExecuteCommand();
        }
        _logger.LogInformation("The file archiving process is complete - {0}{1}    Number of files archived: {2} ({3}MB)", 
            DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss zzz"),
            Environment.NewLine,
            totalFilesArchived,
            Math.Round((double)totalBytesArchived / (1024*1024), 2));
    }

    private FileInfo ParsePathToZipDelegate(ArgumentResult result)
    {
        var filePath = String.Empty;
        if (result.Tokens.Count < 1)
        {
            // Grab the value from appsettings.json 
            filePath = _config.PathTo7Zip;
        }
        else
        {
            // Value from command line
            filePath = result.Tokens.Single().Value;
        }
        if (!File.Exists(filePath))
        {
            result.ErrorMessage = $"File, {filePath}, could not be found. Please locate the 7zip executable on your system and provide it to the --path-to-7zip option. Alternatively, update appsettings.json.";
        }
        
        return new FileInfo(filePath);
    }

    private IEnumerable<DirectoryInfo> ParseDirectoriesDelegate(ArgumentResult result)
    {
        var filteredDirectories = new List<DirectoryInfo>();
        foreach (var t in result.Tokens)
        {
            var filePath = t.Value;
            var exists = Directory.Exists(filePath);

            if (exists)
            {
                filteredDirectories.Add(new DirectoryInfo(filePath));
            }
            else
            {
                result.ErrorMessage = $"Directory, {filePath}, could not be found. Please ensure the directories to be processed exist.";
            }
        }
        
        return filteredDirectories;
    }
}