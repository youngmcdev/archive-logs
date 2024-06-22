using System.CommandLine;

namespace mcy.Tools.Services;

public interface IRootCommandService
{
    RootCommand BuildRootCommand();
}