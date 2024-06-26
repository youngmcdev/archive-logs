using mcy.Tools.Commands;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.Options;
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
            .AddScoped<IOptionFactoryBool, OptionFactoryBool>()
            .AddScoped<IOptionFactoryScalar<string>, OptionFactoryScalar<string>>()
            .AddScoped<IOptionFactoryScalar<int>, OptionFactoryScalar<int>>()
            .AddScoped<IOptionFactoryScalar<double>, OptionFactoryScalar<double>>()
            .AddScoped<IOptionFactoryScalar<FileInfo>, OptionFactoryScalar<FileInfo>>()
            .AddScoped<IOptionFactoryScalar<IEnumerable<DirectoryInfo>>, OptionFactoryScalar<IEnumerable<DirectoryInfo>>>()
            .AddScoped<IOptionFactoryScalar<ArchiveLogFileTypes>, OptionFactoryScalar<ArchiveLogFileTypes>>()
            .AddScoped<ICommandExecutor, CommandExecutor>();

        var host = builder.Build();
        host.Run();
    }
}