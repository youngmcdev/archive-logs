using System.CommandLine;

namespace mcy.Tools;

public class ArchiveRootCommandService: IRootCommandService
{
    private readonly ILogger<ArchiveRootCommandService> _logger;

    public ArchiveRootCommandService(ILogger<ArchiveRootCommandService> logger)
    {
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

        var directoriesOption = new Option<IEnumerable<DirectoryInfo>>(
            name: "--directories",
            isDefault:true,
            parseArgument: result => {
                var filteredDirectories = new List<DirectoryInfo>();
                foreach(var t in result.Tokens)
                {
                    var filePath = t.Value;
                    var exists = Directory.Exists(filePath);
                    _logger.LogInformation("   Token:{0}; DirectoryExists:{1}", filePath, exists);
                    if(exists)
                    { 
                        filteredDirectories.Add(new DirectoryInfo(filePath));
                    }
                    else
                    {
                        result.ErrorMessage = $"Directory, {filePath}, does not exist.";
                    }
                }
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
                if (result.Tokens.Count < 1)
                {
                    // Grab the value from appsettings.json and check that it exits.
                    return new FileInfo("sample-quotes.txt"); // default value
                }
                string? filePath = result.Tokens.Single().Value;
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = $"File, {filePath}, does not exist.";
                    return null;
                }
                return new FileInfo(filePath);
            },
            description: "Path to the 7-zip program. Overrides the value in appSettings.json.");

        
        var archiveCommand = new Command("archive", "Archive log files and optionally delete the original files.")
        {
            dryRunOption,
            deleteFilesOption,
            directoriesOption,
            logFileTypeOption,
            pathTo7zipOption
        };
        return new RootCommand("A program for creating archives."){archiveCommand};
    }
}

public enum ArchiveLogFileTypes
{
    None = 0,
    IIS,
    /// <summary>
    /// Date in Matches[2]. Date in yyyyMM format.
    /// </summary>
    SplashPageManager,
    /// <summary>
    /// Date in Matches[2]. No date parsing requiered - already in yyyy-MM-DD
    /// </summary>
    ApiMoneyDesktop,
    /// <summary>
    /// Date in Matches[2]. Date in yyyyMMDD format and parsed out into yyyy-MM-DD.
    /// </summary>
    SmsItsMe
}