using System.CommandLine;
using System.Text;
using mcy.Tools.Infrastructure;
using mcy.Tools.Infrastructure.Cli;
using mcy.Tools.Models;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Options;
using mcy.Tools.Services;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Commands;

public interface ICommandFactory
{
    abstract Command CreateCommand();
}
public abstract class CommandFactory: ICommandFactory
{
    protected Command _command;

    public abstract Command CreateCommand();
}

public interface ICommandFactoryArchive: ICommandFactory{}
public class CommandFactoryArchive: CommandFactory, ICommandFactoryArchive
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<CommandFactoryArchive> _logger;
    private readonly IOptionValidationService _optionValidationService;
    private readonly IOptionFactoryBool _boolOptionFactory;
    private readonly IOptionFactoryScalar<FileInfo> _fileOptionFactory;
    private readonly IOptionFactoryScalar<IEnumerable<DirectoryInfo>> _directoriesOptionFactory;
    private readonly IOptionFactoryScalar<ArchiveLogFileTypes> _fileTypeOptionFactory;
    public CommandFactoryArchive(
        IOptions<ArchiveOptions> config,
        ILogger<CommandFactoryArchive> logger,
        IOptionValidationService optionValidationService,
        IOptionFactoryBool boolOptionFactory,
        IOptionFactoryScalar<FileInfo> fileOptionFactory,
        IOptionFactoryScalar<IEnumerable<DirectoryInfo>> filesOptionFactory,
        IOptionFactoryScalar<ArchiveLogFileTypes> fileTypeOptionFactory
    )
    {
        _config = config.Value;
        _logger = logger;
        _optionValidationService = optionValidationService;
        _boolOptionFactory = boolOptionFactory;
        _fileOptionFactory = fileOptionFactory;
        _directoriesOptionFactory = filesOptionFactory;
        _fileTypeOptionFactory = fileTypeOptionFactory;
    }

    public override Command CreateCommand()
    {
        var dryRunOption = _boolOptionFactory.CreateOption(new CreateOptionRequestBool{
            Name = "--dry-run",
            Description = "If true, the archiving will to occur. However, the directories and files will be iterated over, and logging will be written. Defaults to false."
        });

        var deleteFilesOption = _boolOptionFactory.CreateOption(new CreateOptionRequestBool{
            Name = "--delete-files",
            Alias = "-del",
            Description = "If true, the original, archived files will be deleted. Defaults to false."
        });
        
        var directoriesFromConfigurationFile = _boolOptionFactory.CreateOption(new CreateOptionRequestBool{
            Name = "--directories-from-config",
            Description = "If true, then --directories is ignored. Multiple sets of directories and their respective --log-file-type values may be specified in appsettings.json. This is a way to queue multiple runs of the archive command. Defaults to false."
        });

        var directoriesOption = _directoriesOptionFactory.CreateOption(new CreateOptionRequestScalar<IEnumerable<DirectoryInfo>>
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
                foreach(var t in result.Tokens)
                {
                    var filePath = t.Value;
                    sb.AppendFormat("{0}{1}", (sb.Length > 0 ? ", ": String.Empty), filePath);
                    var exists = Directory.Exists(filePath);
                    
                    if(exists)
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

        var pathTo7zipOption = _fileOptionFactory.CreateOption(new CreateOptionRequestScalar<FileInfo>{
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

        var logFileTypeOption = _fileTypeOptionFactory.CreateOption(new CreateOptionRequestScalar<ArchiveLogFileTypes>{
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
            
        }, new ArchiveCommandHandlerOptionsBinder(dryRunOption, deleteFilesOption, directoriesFromConfigurationFile, directoriesOption, logFileTypeOption, pathTo7zipOption));

        return _command;
    }

}