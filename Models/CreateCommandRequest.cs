using System.CommandLine;

namespace mcy.Tools.Models;

public class CreateCommandRequest : BaseCreateCommandRequest
{
    public string Name {get;set;} = String.Empty;
}

public class CreateRootCommandRequest : BaseCreateCommandRequest
{
    public List<Option> Options {get;set;} = new();
}

public abstract class BaseCreateCommandRequest
{
    public string Description {get;set;} = String.Empty;
    public string Alias {get;set;} = String.Empty;
    public List<Command> Commands {get;set;} = new();
}