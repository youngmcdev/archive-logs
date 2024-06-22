using System.CommandLine;
using System.Text;
using System.Text.Json;
using mcy.Tools.Infrastructure;
using mcy.Tools.Infrastructure.Cli;
using mcy.Tools.Models.AppSettings;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Services;

public class ArchiveRootCommandService: IRootCommandService
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveRootCommandService> _logger;

    public ArchiveRootCommandService(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveRootCommandService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public RootCommand BuildRootCommand()
    {
        var dryRunOption = new Option<bool>(
            name: "--dry-run",
            description: "If true, the archiving will to occur. However, the directories and files will be iterated over, and logging will be written. Defaults to false.");

        var deleteFilesOption = new Option<bool>(
            name: "--delete-files",
            description: "If true, the original, archived files will be deleted. Defaults to false.");
        var directoriesFromConfigurationFile = new Option<bool>(
            name: "--directories-from-config",
            description: "If true, then --directories is ignored. Multiple sets of directories and their respective --log-file-type values may be specified in appsettings.json. This is a way to queue multiple runs of the archive command. Defaults to false."
        );
        var directoriesOption = new Option<IEnumerable<DirectoryInfo>>(
            name: "--directories",
            isDefault:true,
            parseArgument: result => {
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
            },
            description: "A list of directories to search for log files. An archive file will be left in each directory where log files are found.")
            { 
                IsRequired = true, 
                // Allows --search-terms Presto Hemispheres "A Farwell to Kings" Signals "Grace Under Pressure"
                AllowMultipleArgumentsPerToken = true
            };

        var logFileTypeOption = new Option<ArchiveLogFileTypes>(
            name: "--log-file-type", 
            description: "The type of log file that will be looked for in the directories specified. Other file will be ignored. A file type uses a regular expression to compare to file names and expects the date to be in the file name at a specific, relative location in the file name."){
                IsRequired = true
            };
        
        var pathTo7zipOption = new Option<FileInfo>(
            name: "--path-to-7zip", 
            isDefault: true,
            parseArgument: result => {
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
            },
            description: "Path to the 7-zip program. Overrides the value in appSettings.json.");

        
        var archiveCommand = new Command("archive", "Archive log files and optionally delete the original files.")
        {
            dryRunOption,
            deleteFilesOption,
            directoriesOption,
            logFileTypeOption,
            pathTo7zipOption,
            directoriesFromConfigurationFile
        };

        archiveCommand.SetHandler(archiveCommandHandlerOptions => 
        {
            _logger.LogInformation("Archive Command Handler Options: {0}", archiveCommandHandlerOptions.ToString());
        }, new ArchiveCommandHandlerOptionsBinder(dryRunOption, deleteFilesOption, directoriesFromConfigurationFile, directoriesOption, logFileTypeOption, pathTo7zipOption));
        
        return new RootCommand("A program for creating archives."){archiveCommand};
    }
}
