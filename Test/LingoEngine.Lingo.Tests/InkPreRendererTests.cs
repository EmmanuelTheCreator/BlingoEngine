using LingoEngine.Primitives;
using LingoEngine.Tools;
using Xunit;

public class InkPreRendererTests
{
    [Fact]
    public void BlendInkProducesOpaquePreMultipliedPixels()
    {
        var rgba = new byte[] { 100, 150, 200, 128 };
        var result = InkPreRenderer.Apply(rgba, LingoInkType.Blend, new LingoColor(0, 0, 0));
        Assert.Equal(50, result[0]);
        Assert.Equal(75, result[1]);
        Assert.Equal(100, result[2]);
        Assert.Equal(255, result[3]);
    }
}
