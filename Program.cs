using mcy.CmdTools.CliCommands;
using mcy.CmdTools.Models.AppSettings;
using mcy.CmdTools.CliOptions;
using mcy.CmdTools.Services;
using mcy.CmdTools.Commands.Archive;
using mcy.CmdTools.Infrastructure.Archive;
using Serilog;
using mcy.CmdTools.Strategies.Archive;

namespace mcy.CmdTools;

public class Program
{
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        // TODO: Send logging to a file.
        Log.Logger = new LoggerConfiguration()
            //.WriteTo.Console()
            //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("**********    STARTING THE APPLICATION    **********", Environment.NewLine);
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.Configure<ArchiveOptions>(builder.Configuration.GetSection(ArchiveOptions.Archive));
            builder.Services.AddHostedService<Worker>()
                .AddSerilog()
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
                .AddScoped<ICommandExecutor, CommandExecutor>()
                .AddScoped<IArchiveVerifyFileStrategy_yyMMdd, ArchiveVerifyFileStrategy_yyMMdd>()
                .AddScoped<IArchiveVerifyFileStrategy_yyyyMM, ArchiveVerifyFileStrategy_yyyyMM>()
                .AddScoped<IArchiveVerifyFileStrategy_yyyyMMdd, ArchiveVerifyFileStrategy_yyyyMMdd>()
                .AddScoped<IArchiveVerifyFileStrategy_yyyy_MM_dd, ArchiveVerifyFileStrategy_yyyy_MM_dd>();

            
            var host = builder.Build();
            host.Run();
        }
        catch(Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}