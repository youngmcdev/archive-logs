using System.CommandLine;
using mcy.Tools.Models;

namespace mcy.Tools.CliOptions;

public interface IBoolCliOptionFactory
{
    Option<bool> CreateOption(CreateBoolCliOptionRequest request);
}

public class BoolCliOptionFactory : BaseCliOptionFactory<bool, CreateBoolCliOptionRequest>, IBoolCliOptionFactory
{
    public override Option<bool> CreateOption(CreateBoolCliOptionRequest request)
    {
        _option = new Option<bool>(request.Name, request.Description);
        if (!string.IsNullOrWhiteSpace(request.Alias)) _option.AddAlias(request.Alias);
        return _option;
    }
}
