using PairOfEmployees.Server.Services.Parsing;

namespace PairOfEmployees.Server.Tests;

public sealed class AssignmentDateParserTests
{
    private readonly AssignmentDateParser parser = new();

    [Theory]
    [InlineData("2024-02-29")]
    [InlineData("29.02.2024")]
    [InlineData("29/2/2024")]
    [InlineData("2/29/2024")]
    [InlineData("29 February 2024")]
    [InlineData("February 29, 2024")]
    [InlineData("29 февруари 2024")]
    [InlineData("  29.02.2024  ")]
    public void TryParse_SupportedFormat_ReturnsDate(string input)
    {
        Assert.True(parser.TryParse(input, out var date));
        
        Assert.Equal(new DateOnly(2024, 2, 29), date);
    }

    [Fact]
    public void TryParse_AmbiguousDate_PrefersDayBeforeMonth()
    {
        Assert.True(parser.TryParse("03/04/2024", out var date));
        
        Assert.Equal(new DateOnly(2024, 4, 3), date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NULL")]
    [InlineData("2023-02-29")]
    [InlineData("2024-13-01")]
    [InlineData("not a date")]
    public void TryParse_InvalidDate_ReturnsFalse(string input)
    {
        Assert.False(parser.TryParse(input, out var date));
        
        Assert.Equal(default, date);
    }
}
