using System.CommandLine;
using mcy.CmdTools.Models;
namespace mcy.CmdTools.CliOptions;

public abstract class BaseCliOptionFactory<TReturn, TRequest> where TRequest : BaseCreateCliOptionRequest
{
    protected Option<TReturn> _option;

    public abstract Option<TReturn> CreateOption(TRequest request);
}
