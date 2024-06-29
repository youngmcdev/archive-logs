namespace mcy.CmdTools;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHost _host;
    private readonly ICommandExecutor _commandExecutor;

    public Worker(ILogger<Worker> logger, IHost host, ICommandExecutor commandExecutor)
    {
        _logger = logger;
        _host = host;
        _commandExecutor = commandExecutor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        var response = _commandExecutor.Run(args);
        _logger.LogInformation("    ** Command response: {0} **", response);
        _host.StopAsync();
    }
}
