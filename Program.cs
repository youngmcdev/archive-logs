using mcy.Tools.CliCommands;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models.AppSettings;
using mcy.Tools.CliOptions;
using mcy.Tools.Services;
using mcy.Tools.Commands;
namespace mcy.Tools.ArchiveLogs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<ArchiveOptions>(builder.Configuration.GetSection(ArchiveOptions.Archive));
        builder.Services.AddHostedService<Worker>()
            .AddScoped<IRootCommandService, RootCommandService>()
            .AddScoped<IRootCliCommandFactory, RootCliCommandFactory>()
            .AddScoped<ICommandFactoryArchive, ArchiveCommandFactory>()
            .AddScoped<IArchiveCommandHandler, ArchiveCommandHandler>()
            .AddScoped<ICliOptionValidationService, CliOptionValidationService>()
            .AddScoped<IZipCommand, ZipCommand>()
            .AddScoped<IBoolCliOptionFactory, BoolCliOptionFactory>()
            .AddScoped<ICliOptionFactory<string>, CliOptionFactory<string>>()
            .AddScoped<ICliOptionFactory<int>, CliOptionFactory<int>>()
            .AddScoped<ICliOptionFactory<double>, CliOptionFactory<double>>()
            .AddScoped<ICliOptionFactory<FileInfo>, CliOptionFactory<FileInfo>>()
            .AddScoped<ICliOptionFactory<IEnumerable<DirectoryInfo>>, CliOptionFactory<IEnumerable<DirectoryInfo>>>()
            .AddScoped<ICliOptionFactory<ArchiveLogFileTypes>, CliOptionFactory<ArchiveLogFileTypes>>()
            .AddScoped<ICommandExecutor, CommandExecutor>();

        var host = builder.Build();
        host.Run();
    }
}