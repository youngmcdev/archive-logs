﻿using System.CommandLine;

namespace mcy.CmdTools;

class McyProg
{
    public static async Task<int> McyProg_Main(string[] args)
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "An option whose argument is parsed as a FileInfo",
            isDefault: true, // This must be true for the parseArgument delegate to be executed when no value is provide for --file on the command line.
            parseArgument: result =>
            {
                // Using ParseArgument<T> to provide custom parsing, validation, and error handling.
                if (result.Tokens.Count == 0)
                {
                    return new FileInfo("sample-quotes.txt"); // default value

                }
                string? filePath = result.Tokens.Single().Value;
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "File does not exist";
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            });

        var delayOption = new Option<int>(
            name: "--delay",
            description: "Delay between lines, specified as milliseconds per character in a line.",
            getDefaultValue: () => 42);

        var fgcolorOption = new Option<ConsoleColor>(
            name: "--fgcolor",
            description: "Foreground color of text displayed on the console.",
            getDefaultValue: () => ConsoleColor.White);

        var lightModeOption = new Option<bool>(
            name: "--light-mode",
            description: "Background color of text displayed on the console: default is black, light mode is white.");

        var searchTermsOption = new Option<string[]>(
            name: "--search-terms",
            description: "Strings to search for when deleting entries.")
            { 
                IsRequired = true, 
                // Allows --search-terms Presto Hemispheres "A Farwell to Kings" Signals "Grace Under Pressure"
                AllowMultipleArgumentsPerToken = true 
            };

        var quoteArgument = new Argument<string>(
            name: "quote",
            description: "Text of quote.");

        var bylineArgument = new Argument<string>(
            name: "byline",
            description: "Byline of quote.");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        rootCommand.AddGlobalOption(fileOption);

        var quotesCommand = new Command("quotes", "Work with a file that contains quotes.");
        rootCommand.AddCommand(quotesCommand);

        var readCommand = new Command("read", "Read and display the file.")
            {
                delayOption,
                fgcolorOption,
                lightModeOption
            };
        quotesCommand.AddCommand(readCommand);

        var deleteCommand = new Command("delete", "Delete lines from the file.");
        deleteCommand.AddOption(searchTermsOption);
        quotesCommand.AddCommand(deleteCommand);

        var addCommand = new Command("add", "Add an entry to the file.");
        addCommand.AddArgument(quoteArgument);
        addCommand.AddArgument(bylineArgument);
        addCommand.AddAlias("insert");
        quotesCommand.AddCommand(addCommand);

        readCommand.SetHandler(async (file, delay, fgcolor, lightMode) =>
            {
                await ReadFile(file!, delay, fgcolor, lightMode);
            },
            fileOption, delayOption, fgcolorOption, lightModeOption);

        deleteCommand.SetHandler((file, searchTerms) =>
            {
                DeleteFromFile(file!, searchTerms);
            },
            fileOption, searchTermsOption);
        
        addCommand.SetHandler((file, quote, byline) =>
            {
                AddToFile(file!, quote, byline);
            },
            fileOption, quoteArgument, bylineArgument);

        return await rootCommand.InvokeAsync(args);
    }

    internal static async Task ReadFile(
        FileInfo file, int delay, ConsoleColor fgColor, bool lightMode)
    {
        Console.BackgroundColor = lightMode ? ConsoleColor.DarkYellow : ConsoleColor.Black;
        Console.ForegroundColor = fgColor;
        List<string> lines = File.ReadLines(file.FullName).ToList();
        foreach (string line in lines)
        {
            Console.WriteLine(line);
            await Task.Delay(delay * line.Length);
        };
    }

    internal static void DeleteFromFile(FileInfo file, string[] searchTerms)
    {
        Console.WriteLine("Deleting from file");
        File.WriteAllLines(
            file.FullName, File.ReadLines(file.FullName)
                .Where(line => searchTerms.All(s => !line.Contains(s))).ToList());
    }
    internal static void AddToFile(FileInfo file, string quote, string byline)
    {
        Console.WriteLine("Adding to file");
        using StreamWriter? writer = file.AppendText();
        writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{quote}");
        writer.WriteLine($"{Environment.NewLine}-{byline}");
        writer.Flush();
    }
}

//https://anthonysimmon.com/true-dependency-injection-with-system-commandline/