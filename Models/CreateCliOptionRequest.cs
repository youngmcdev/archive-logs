using System.CommandLine.Parsing;
namespace mcy.Tools.Models;

public class CreateCliOptionRequest<T>: BaseCreateCliOptionRequest
{
    public bool IsRequired {get;set;}
    public bool IsDefault {get;set;}
    public bool AllowMultipleArgumentsPerToken {get;set;}
    public ParseArgument<T>? ParseArgumentDelegate {get;set;}
}