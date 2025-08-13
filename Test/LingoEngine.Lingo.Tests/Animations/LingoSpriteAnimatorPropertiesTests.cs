using LingoEngine.Animations;
using LingoEngine.Primitives;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class LingoSpriteAnimatorPropertiesTests
{
    [Fact]
    public void BoundingBoxIncludesPositionAnimation()
    {
        var props = new LingoSpriteAnimatorProperties();
        props.AddKeyFrame((10, 100, 0));

        var box = props.GetBoundingBox(new LingoPoint(0, 0), LingoRect.New(0, 0, 10, 10), 10, 10);

        Assert.Equal(new LingoRect(0, 0, 110, 10), box);
    }
}
