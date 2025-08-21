using System.Collections.Generic;
using AbstUI.Primitives;
using LingoEngine.Primitives;
using LingoEngine.Tools;
using Xunit;

public class InkPreRendererTests
{
    private static byte[] Rgba(byte r, byte g, byte b, byte a) => new[] { r, g, b, a };

    private static byte[] Apply(byte[] rgba, LingoInkType ink) =>
        InkPreRenderer.Apply(rgba, ink, new AColor(0, 0, 0));

    public static IEnumerable<object[]> ApplyCases()
    {
        yield return new object[]
        {
            LingoInkType.Copy,
            Rgba(100, 150, 200, 128),
            Rgba(100, 150, 200, 128)
        };
        yield return new object[]
        {
            LingoInkType.Copy,
            Rgba(100, 150, 200, 0),
            Rgba(100, 150, 200, 0)
        };
        yield return new object[]
        {
            LingoInkType.Add,
            Rgba(100, 150, 200, 128),
            Rgba(100, 150, 200, 128)
        };
        yield return new object[]
        {
            LingoInkType.Add,
            Rgba(100, 150, 200, 0),
            Rgba(100, 150, 200, 0)
        };
        yield return new object[]
        {
            LingoInkType.Blend,
            Rgba(100, 150, 200, 128),
            Rgba(50, 75, 100, 255)
        };
        yield return new object[]
        {
            LingoInkType.Blend,
            Rgba(100, 150, 200, 0),
            Rgba(0, 0, 0, 255)
        };
    }

    [Theory]
    [MemberData(nameof(ApplyCases))]
    public void ApplyProducesExpectedPixels(LingoInkType ink, byte[] input, byte[] expected)
    {
        var result = Apply(input, ink);
        Assert.Equal(expected, result);
    }
}

