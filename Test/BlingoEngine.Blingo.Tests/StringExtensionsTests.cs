using BlingoEngine.Primitives;
using Xunit;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("A", 65)]
    [InlineData("", 0)]
    public void CharToNumReturnsAscii(string input, int expected)
    {
        Assert.Equal(expected, input.CharToNum());
    }
}

