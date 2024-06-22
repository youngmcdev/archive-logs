using System.CommandLine;

namespace mcy.Tools;

public interface IRootCommandService
{
    RootCommand BuildRootCommand();
}