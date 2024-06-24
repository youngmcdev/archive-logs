using mcy.Tools.Commands;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Services;
namespace mcy.Tools.ArchiveLogs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<ArchiveOptions>(builder.Configuration.GetSection(ArchiveOptions.Archive));
        builder.Services.AddHostedService<Worker>()
            .AddScoped<IRootCommandService, ArchiveRootCommandService>()
            .AddScoped<IArchiveCommandHandler, ArchiveCommandHandler>()
            .AddScoped<IOptionValidationService, OptionValidationService>()
            .AddScoped<IExecute7zip, Execute7zip>()
            .AddScoped<ICommandExecutor, CommandExecutor>();

        var host = builder.Build();
        host.Run();
    }
}