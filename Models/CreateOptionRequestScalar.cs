using System.CommandLine.Parsing;
namespace mcy.Tools.Models;

public class CreateOptionRequestScalar<T>: BaseCreateOptionRequest
{
    public bool IsRequired {get;set;}
    public bool IsDefault {get;set;}
    public bool AllowMultipleArgumentsPerToken {get;set;}
    public ParseArgument<T>? ParseArgumentDelegate {get;set;}
}