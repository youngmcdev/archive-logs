using System;
using Xunit;
using mcy.CmdTools.CliOptions;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using mcy.CmdTools.Models.CliOptions;
using System.CommandLine;

namespace mcy.CmdTools.Tests.CliOptions;

public class OptionFactoryTests
{
    private readonly IBoolCliOptionFactory _boolCliOptionFactory;
    private readonly ICliOptionFactory<string> _stringOptionFactory;
    const string _OptionName = "some-option",
        _OptionDescription = "This option will enable some desirable feature.",
        _OptionAias = "-so";

    public OptionFactoryTests()
    {
        _boolCliOptionFactory = new BoolCliOptionFactory();
        _stringOptionFactory = new CliOptionFactory<string>();
    }

    [Fact]
    public void TestAssignmentOfNameDescriptionAlias_Boolean()
    {
        var option = GetBoolCommandLineOption();
        Assert.Equal(_OptionName, option.Name);
        Assert.Equal(_OptionDescription, option.Description);
        Assert.True(option.HasAlias(_OptionAias));
    }

    [Fact]
    public void TestAssignmentOfNameDescriptionAlias_String()
    {
        var option = GetStringCommandLineOption();
        Assert.Equal(_OptionName, option.Name);
        Assert.Equal(_OptionDescription, option.Description);
        Assert.True(option.HasAlias(_OptionAias));
    }

    private Option<bool> GetBoolCommandLineOption()
    {
        var optionRequest = new CreateBoolCliOptionRequest{Name = string.Format("--{0}", _OptionName), Description = _OptionDescription, Alias = _OptionAias};
        return _boolCliOptionFactory.CreateOption(optionRequest);
    }

    private Option<string> GetStringCommandLineOption()
    {
        var optionRequest = new CreateCliOptionRequest<string>{Name = string.Format("--{0}", _OptionName), Description = _OptionDescription, Alias = _OptionAias};
        return _stringOptionFactory.CreateOption(optionRequest);
    }
}
