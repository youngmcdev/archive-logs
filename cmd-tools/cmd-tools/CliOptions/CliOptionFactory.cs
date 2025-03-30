using System.CommandLine;
using mcy.CmdTools.Models.CliOptions;

namespace mcy.CmdTools.CliOptions;

public interface ICliOptionFactory<T>
{
    Option<T> CreateOption(CreateCliOptionRequest<T> request);
}

public class CliOptionFactory<T> : BaseCliOptionFactory<T, CreateCliOptionRequest<T>>, ICliOptionFactory<T>
{
    public override Option<T> CreateOption(CreateCliOptionRequest<T> request)
    {
        if (request.ParseArgumentDelegate is null)
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

        if (!string.IsNullOrWhiteSpace(request.Alias)) _option.AddAlias(request.Alias);

        return _option;
    }
}
