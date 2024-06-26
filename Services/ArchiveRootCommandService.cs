using System.CommandLine;
using System.Text;
using mcy.Tools.Commands;
using mcy.Tools.Infrastructure;
using mcy.Tools.Infrastructure.Cli;
using mcy.Tools.Models;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Options;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Services;

public class ArchiveRootCommandService: IRootCommandService
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<ArchiveRootCommandService> _logger;
    private readonly IRootCommandFactory _rootCommandFactory;
    private readonly ICommandFactoryArchive _archiveCommandFactory;
    
    public ArchiveRootCommandService(
        IOptions<ArchiveOptions> config,
        ILogger<ArchiveRootCommandService> logger,
        IRootCommandFactory rootCommandFactory,
        ICommandFactoryArchive archiveCommandFactory)
    {
        _config = config.Value;
        _logger = logger;
        _rootCommandFactory = rootCommandFactory;
        _archiveCommandFactory = archiveCommandFactory;
    }

    public RootCommand BuildRootCommand()
    {
        var archive = _archiveCommandFactory.CreateCommand();
        var rootCommand = _rootCommandFactory.CreateCommand(new CreateRootCommandRequest
        {
            Description = "Various Tools.",
            Alias = "mct",
            Commands = new List<Command>{archive}
        });
        return rootCommand;
    }
}
