using System.Diagnostics.Metrics;
using mcy.Tools.Models;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.CliCommands;
using mcy.Tools.Commands;
namespace mcy.Tools.Services;

public interface IArchiveCommandHandler: ICommand
{
    ArchiveCliCommandHandlerOptions HandlerOptions {get;}
    void SetHandlerOptions(ArchiveCliCommandHandlerOptions handlerOptions);
}

public class ArchiveCommandHandler: IArchiveCommandHandler
{
    private readonly ILogger<ArchiveCommandHandler> _logger;
    private readonly ArchiveOptions _config;
    private readonly IZipCommand _7zipCommand;
    private readonly string _homeDirectory;
    public ArchiveCommandHandler(
        ILogger<ArchiveCommandHandler> logger,
        ArchiveOptions config,
        IZipCommand cmd7zip)
    {
        _logger = logger;
        _config = config;
        _7zipCommand = cmd7zip;
        _homeDirectory = Directory.GetCurrentDirectory();
    }
    public ArchiveCliCommandHandlerOptions HandlerOptions {get; protected set;}
    public void SetHandlerOptions(ArchiveCliCommandHandlerOptions handlerOptions) => HandlerOptions = handlerOptions;
    /* 
      Iterate over directories - Foreach directory
        - Check that it exits
        - Change current directory to that location
        - Get list of files in directory that match the regex - I think this is the "strategy"
        - Get stats: # of files to be archived and # of bytes
        - Create archive file name and full path
        - Determine whether original files will be deleted and build 7zip command
        - Execute 7zip command
        - Change current directory back to original

    */
    public void Execute()
    {
        foreach(var directoryInfo in HandlerOptions.Directories)
        {
            var directoryPath = directoryInfo.FullName;
            if(!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory, {0}, was not found. It will be skipped. Log files will not be archived.", directoryPath);
                continue;
            }
            Directory.SetCurrentDirectory(directoryPath);
            ArchiveFileSource fileNames = new(); // use strategy to get file names
            
            var archiveFileName = string.Format(Constants.ArchiveFileNameTemplate, 
                directoryInfo.Name, 
                DateTime.Today.AddMonths(_config.ArchiveFileNameMonthOffset).ToString("yyyyMM"));

//            _7zipCommand.SetFiles(fileNames.Files[0].);

            Directory.SetCurrentDirectory(_homeDirectory);
        }
        
    }
}