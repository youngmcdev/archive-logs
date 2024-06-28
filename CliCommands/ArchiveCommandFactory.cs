using System.CommandLine;
using mcy.Tools.CliOptions;
using mcy.Tools.Infrastructure.Cli;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Models;
using mcy.Tools.Services;
using System.Text;
using Microsoft.Extensions.Options;
using mcy.Tools.Commands;

namespace mcy.Tools.CliCommands;

public interface IArchiveCliCommandFactory : ICliCommandFactory { }
public class ArchiveCliCommandFactory : CliCommandFactory, IArchiveCliCommandFactory
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveCliCommandFactory> _logger;
    private readonly ICliOptionValidationService _optionValidationService;
    private readonly IBoolCliOptionFactory _boolOptionFactory;
    private readonly ICliOptionFactory<FileInfo> _fileOptionFactory;
    private readonly ICliOptionFactory<IEnumerable<DirectoryInfo>> _directoriesOptionFactory;
    private readonly ICliOptionFactory<ArchiveLogFileTypes> _fileTypeOptionFactory;
    private readonly IArchiveActions _achiveActions;
    public ArchiveCliCommandFactory(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveCliCommandFactory> logger,
        ICliOptionValidationService optionValidationService,
        IBoolCliOptionFactory boolOptionFactory,
        ICliOptionFactory<FileInfo> fileOptionFactory,
        ICliOptionFactory<IEnumerable<DirectoryInfo>> filesOptionFactory,
        ICliOptionFactory<ArchiveLogFileTypes> fileTypeOptionFactory,
        IArchiveActions achiveActions
    )
    {
        _config = config.Value;
        _logger = logger;
        _optionValidationService = optionValidationService;
        _boolOptionFactory = boolOptionFactory;
        _fileOptionFactory = fileOptionFactory;
        _directoriesOptionFactory = filesOptionFactory;
        _fileTypeOptionFactory = fileTypeOptionFactory;
        _achiveActions = achiveActions;
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
            ParseArgumentDelegate = result => {
                var filteredDirectories = new List<DirectoryInfo>();
                var sb = new StringBuilder();
                foreach (var t in result.Tokens)
                {
                    var filePath = t.Value;
                    sb.AppendFormat("{0}{1}", (sb.Length > 0 ? ", " : String.Empty), filePath);
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
                _logger.LogInformation("Directories to be processed: {0}", sb.ToString());
                return filteredDirectories;
            }
        });

        var pathTo7zipOption = _fileOptionFactory.CreateOption(new CreateCliOptionRequest<FileInfo>
        {
            Name = "--path-to-zip",
            Alias = "-7z",
            Description = "Path to the 7-zip program. Overrides the value in appSettings.json.",
            IsDefault = true,
            ParseArgumentDelegate = result => {
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
                _logger.LogInformation("7zip location: {0}", filePath);
                return new FileInfo(filePath);
            }
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

        _command.SetHandler(archiveCommandHandlerOptions =>
        {
            _logger.LogInformation("Archive Command Handler Options: {0}", archiveCommandHandlerOptions.ToString());
            var currDirectory = archiveCommandHandlerOptions.Directories[0].FullName; // TODO: Loop over the directories
            var archiveInvoker = new ArchiveInvoker();
            archiveInvoker.SetCommand(new ArchiveBuildSourceCommand(
                new BulidArchiveSourceRequest{
                    LogFileType = archiveCommandHandlerOptions.ArchiveLogFileType,
                    PathToDirectory = currDirectory
                }, 
                _achiveActions));
            archiveInvoker.ExecuteCommand();
            if(_achiveActions.ArchiveSource.Files.Count > 0) 
            {
                Directory.SetCurrentDirectory(currDirectory);
                archiveInvoker.SetCommand(new ArchiveFilesCommand(
                    new ArchiveFilesRequest{
                        IsDryRun = archiveCommandHandlerOptions.IsDryRun,
                        IsDeleteFiles = archiveCommandHandlerOptions.IsDeleteFiles,
                        PathTo7Zip = archiveCommandHandlerOptions.PathTo7Zip
                    }, 
                    _achiveActions));
                archiveInvoker.ExecuteCommand();
            }

            
        }, new ArchiveCliCommandHandlerOptionsBinder(dryRunOption, deleteFilesOption, directoriesFromConfigurationFile, directoriesOption, logFileTypeOption, pathTo7zipOption));

        return _command;
    }
}