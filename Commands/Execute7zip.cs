using System.Diagnostics;
using mcy.Tools.Models;
using mcy.Tools.Models.AppSettings;

namespace mcy.Tools.Commands;

public interface IExecute7zip: ICommand
{
    string[] Files{get;}
    void SetFiles(string[] files);
}

public class Execute7zip: IExecute7zip
{
    public string[] Files{get;protected set;}
    public void SetFiles(string[] files) => Files = files;
    private readonly ArchiveOptions _config;
    
    public Execute7zip(Execute7zipRequest request)
    {
        
    }

    public void Execute()
    {
        using var process = new Process
        {
            StartInfo = 
            {
                //FileName = _config.PathTo7Zip,
                Arguments = "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        // test case
        process.StartInfo.FileName = @"cmd.exe";
        process.StartInfo.Arguments = @"/c dir";      // print the current working directory information

        process.OutputDataReceived += (_, data) => Console.WriteLine(data.Data);
        process.ErrorDataReceived += (_, data) => Console.WriteLine(data.Data);
        Console.WriteLine("starting");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
        Console.WriteLine($"exit {exited}");

    }
}