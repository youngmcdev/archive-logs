# Patterns used in this project

## Factory

Used to generate CliCommand and CliOption objects.

### Factory Components

- Abstract Target: CommandLine.Option&lt;T&gt;
- Concrete Target: CommandLine.Option&lt;bool&gt;, CommandLine.Option&lt;T&gt; where T is specified at injection
- Abstract Factory: CliOptions.BaseCliOptionFactory
- Concrete Factory: CliOptions.BoolCliOptionFactory, CliOptions.CliOptionFactory

## Command

Used to...

1. Gather the files to be archived in a directory.
2. Execute 7zip in a process.

### Command Components

- Abstract Command: Commands.ICommand
- Concrete Command: Commands.ArchiveBuildSourceCommand, Command.ArchiveFilesCommand
- Receiver: Commands.ArchiveActions
- Invoker: Commands.ArchiveInvoker

## Strategy

Determines the logic that's used to verify whether each file should be included.

### Strategy Components

- Abstract Strategy: Strategies.IArchiveVerifyFileStrategy
- Concrete Strategy: Strategies.ArchiveVerifyFileStrategy_yyMMdd, Strategies.ArchiveVerifyFileStrategy_yyyy_MM_dd, Strategies.ArchiveVerifyFileStrategy_yyyyMM, Strategies.ArchiveVerifyFileStrategy_yyyyMMdd
- Context: Commands.ArchiveActions

## Template

Used to define the different Strategies.

### Template Components

- Abstract Template: ArchiveVerifyFileStrategy
- Concrete Template: Strategies.ArchiveVerifyFileStrategy_yyMMdd, Strategies.ArchiveVerifyFileStrategy_yyyy_MM_dd, Strategies.ArchiveVerifyFileStrategy_yyyyMM, Strategies.ArchiveVerifyFileStrategy_yyyyMMdd
- Client: Commands.ArchiveActions