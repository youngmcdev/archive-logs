using System.CommandLine;

namespace mcy.Tools.Models;

public class CreateCliCommandRequest : BaseCreateCliCommandRequest
{
    public string Name {get;set;} = String.Empty;
}

public class CreateRootCliCommandRequest : BaseCreateCliCommandRequest
{
    public List<Option> Options {get;set;} = new();
}

public abstract class BaseCreateCliCommandRequest
{
    public string Description {get;set;} = String.Empty;
    public string Alias {get;set;} = String.Empty;
    public List<Command> Commands {get;set;} = new();
}