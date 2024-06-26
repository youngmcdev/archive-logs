using System.CommandLine;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using mcy.Tools.Infrastructure;
using mcy.Tools.Models;
namespace mcy.Tools.Options;

public abstract class OptionFactory<TReturn, TRequest> where TRequest : BaseCreateOptionRequest
{
    protected Option<TReturn> _option;

    public abstract Option<TReturn> CreateOption(TRequest request);
}

public interface IOptionFactoryBool
{
    Option<bool> CreateOption(CreateOptionRequestBool request);
}

public class OptionFactoryBool: OptionFactory<bool, CreateOptionRequestBool>, IOptionFactoryBool
{
    public override Option<bool> CreateOption(CreateOptionRequestBool request)
    {
        _option = new Option<bool>(request.Name, request.Description);
        if(!string.IsNullOrWhiteSpace(request.Alias)) _option.AddAlias(request.Alias);
        return _option;
    }
}

public interface IOptionFactoryScalar<T>
{
    Option<T> CreateOption(CreateOptionRequestScalar<T> request);
}

public class OptionFactoryScalar<T>: OptionFactory<T, CreateOptionRequestScalar<T>>, IOptionFactoryScalar<T>
{
    public override Option<T> CreateOption(CreateOptionRequestScalar<T> request)
    {
        if(request.ParseArgumentDelegate is null)
        {
            _option = new Option<T>(
                name: request.Name, 
                description: request.Description)
                {
                    IsRequired = request.IsRequired,
                    AllowMultipleArgumentsPerToken = request.AllowMultipleArgumentsPerToken
                };
        }
        else
        {
            _option = new Option<T>(
                name: request.Name, 
                isDefault: request.IsDefault,
                parseArgument: request.ParseArgumentDelegate,
                description: request.Description)
                {
                    IsRequired = request.IsRequired,
                    AllowMultipleArgumentsPerToken = request.AllowMultipleArgumentsPerToken
                };
        }

        if(!string.IsNullOrWhiteSpace(request.Alias)) _option.AddAlias(request.Alias);
        
        Console.WriteLine("Name:{0} OptionFlag:{1}  RequestFlag:{2}", request.Name, _option.AllowMultipleArgumentsPerToken, request.AllowMultipleArgumentsPerToken);
        return _option;
    }
}

