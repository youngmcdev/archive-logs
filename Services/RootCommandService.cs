using mcy.Tools.CliCommands;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Models;
using System.CommandLine;
using Microsoft.Extensions.Options;

namespace mcy.Tools.Services;

public interface IRootCommandService
{
    RootCommand BuildRootCommand();
}

public class RootCommandService : IRootCommandService
{
    private readonly ArchiveOptions _config;
    private readonly ILogger<RootCommandService> _logger;
    private readonly IRootCliCommandFactory _rootCommandFactory;
    private readonly IArchiveCliCommandFactory _archiveCommandFactory;

    public RootCommandService(
        IOptions<ArchiveOptions> config,
        ILogger<RootCommandService> logger,
        IRootCliCommandFactory rootCommandFactory,
        IArchiveCliCommandFactory archiveCommandFactory)
    {
        _config = config.Value;
        _logger = logger;
        _rootCommandFactory = rootCommandFactory;
        _archiveCommandFactory = archiveCommandFactory;
    }

    public RootCommand BuildRootCommand()
    {
        var archive = _archiveCommandFactory.CreateCommand();
        var rootCommand = _rootCommandFactory.CreateCommand(new CreateRootCliCommandRequest
        {
            Description = "Various Tools.",
            Alias = "mct",
            Commands = new List<Command> { archive }
        });
        return rootCommand;
    }
}