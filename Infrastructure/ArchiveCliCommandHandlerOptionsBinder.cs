using System.CommandLine;
using System.CommandLine.Binding;
using mcy.CmdTools.Models.Archive;

namespace mcy.CmdTools.Infrastructure.Archive;

public class ArchiveCliCommandHandlerOptionsBinder: BinderBase<ArchiveCliCommandHandlerOptions>
{
    private readonly Option<bool> _dryRunOption;
    private readonly Option<bool> _deleteFilesOption;
    private readonly Option<bool> _directoriesFromConfigurationFileOption;
    private readonly Option<IEnumerable<DirectoryInfo>> _directoriesOption;
    private readonly Option<ArchiveLogFileTypes> _archiveLogFileTypeOption;
    private readonly Option<FileInfo> _pathTo7ZipOption;

    public ArchiveCliCommandHandlerOptionsBinder(
        Option<bool> dryRunOption,
        Option<bool> deleteFilesOption,
        Option<bool> directoriesFromConfigurationFileOption,
        Option<IEnumerable<DirectoryInfo>> directoriesOption,
        Option<ArchiveLogFileTypes> archiveLogFileTypeOption,
        Option<FileInfo> pathTo7ZipOption)
    {
        _dryRunOption = dryRunOption;
        _deleteFilesOption = deleteFilesOption;
        _directoriesFromConfigurationFileOption = directoriesFromConfigurationFileOption;
        _directoriesOption = directoriesOption;
        _archiveLogFileTypeOption = archiveLogFileTypeOption;
        _pathTo7ZipOption = pathTo7ZipOption;
    }

    protected override ArchiveCliCommandHandlerOptions GetBoundValue(BindingContext bindingContext) => 
        new ArchiveCliCommandHandlerOptions
        {
            IsDryRun = bindingContext.ParseResult.GetValueForOption(_dryRunOption),
            IsDeleteFiles = bindingContext.ParseResult.GetValueForOption(_deleteFilesOption),
            IsDirectoriesFromConfigurationFile = bindingContext.ParseResult.GetValueForOption(_directoriesFromConfigurationFileOption),
            Directories = bindingContext.ParseResult.GetValueForOption(_directoriesOption)?.ToList() ?? new List<DirectoryInfo>(),
            ArchiveLogFileType = bindingContext.ParseResult.GetValueForOption(_archiveLogFileTypeOption),
            PathTo7Zip = bindingContext.ParseResult.GetValueForOption(_pathTo7ZipOption)
        };
}