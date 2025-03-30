using System;
using Xunit;
using mcy.CmdTools.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using System.Text.RegularExpressions;

namespace mcy.CmdTools.Tests.Infrastructure;

public class UtilityTests
{
    private readonly IUtility _utilities;
    const string _OriginalString = "Let not mercy and truth forsake thee: bind them about thy neck; write them upon the table of thine heart: So shalt thou find favour and good understanding in the sight of God and man.",
        _ReversedString = ".nam dna doG fo thgis eht ni gnidnatsrednu doog dna ruovaf dnif uoht tlahs oS :traeh eniht fo elbat eht nopu meht etirw ;kcen yht tuoba meht dnib :eeht ekasrof hturt dna ycrem ton teL",
        _PalindromeString = "Doc note I dissent A fast never prevents a fatness I diet on cod";
    
    public UtilityTests()
    {
        _utilities = new Utility();
    }

    [Fact]
    public void ShouldReverseAStringCorrectly()
    {
        Assert.Equal(_ReversedString, _utilities.ReverseString(_OriginalString));
    }

    [Fact]
    public void ReversedPalindromeShouldEqualOriginalString()
    {
        var palindromeSansWhiteSpace = Regex.Replace(_PalindromeString, @"\s+", string.Empty);
        Assert.Equal(palindromeSansWhiteSpace.ToUpper(), _utilities.ReverseString(palindromeSansWhiteSpace).ToUpper());
    }
}
