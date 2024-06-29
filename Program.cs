using mcy.CmdTools.CliCommands;
using mcy.CmdTools.Infrastructure;
using mcy.CmdTools.Models.AppSettings;
using mcy.CmdTools.CliOptions;
using mcy.CmdTools.Services;
using mcy.CmdTools.Commands.Archive;

namespace mcy.CmdTools;

public class Program
{
    public static void Main(string[] args)
    {
        // TODO: Add logging to a file.
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<ArchiveOptions>(builder.Configuration.GetSection(ArchiveOptions.Archive));
        builder.Services.AddHostedService<Worker>()
            .AddScoped<IRootCommandService, RootCommandService>()
            .AddScoped<IRootCliCommandFactory, RootCliCommandFactory>()
            .AddScoped<IArchiveCliCommandFactory, ArchiveCliCommandFactory>()
            .AddScoped<IArchiveActions, ArchiveActions>()
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