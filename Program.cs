namespace mcy.Tools.ArchiveLogs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>()
            .AddScoped<ICommandLineService, CommandLineService>();

        var host = builder.Build();
        host.Run();
    }
}