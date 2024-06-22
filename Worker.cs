namespace mcy.Tools.ArchiveLogs;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHost _host;

    public Worker(ILogger<Worker> logger, IHost host)
    {
        _logger = logger;
        _host = host;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hello world!");
        _logger.LogDebug("ExecuteAsync!");
        var args = Environment.GetCommandLineArgs();
        var response = new CommandExecutor(args).Run();
        _logger.LogDebug("Command response: {0}", response);
        _host.StopAsync();
    }
}
