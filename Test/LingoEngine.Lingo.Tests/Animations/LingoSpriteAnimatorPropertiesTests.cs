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

    [Fact]
    public void MoveKeyFrame_MovesDataToNewFrame()
    {
        var props = new LingoSpriteAnimatorProperties();
        props.AddKeyFrame(new LingoKeyFrameSetting(5, position: new LingoPoint(10, 20)));

        props.MoveKeyFrame(5, 8);

        Assert.DoesNotContain(props.Position.KeyFrames, k => k.Frame == 5);
        Assert.Contains(props.Position.KeyFrames, k => k.Frame == 8 && k.Value.Equals(new LingoPoint(10, 20)));
        Assert.DoesNotContain(props.GetKeyFrames()!, k => k.Frame == 5);
        Assert.Contains(props.GetKeyFrames()!, k => k.Frame == 8);
    }

    [Fact]
    public void MoveKeyFrame_ToSameFrame_NoOp()
    {
        var props = new LingoSpriteAnimatorProperties();
        props.AddKeyFrame(new LingoKeyFrameSetting(5, position: new LingoPoint(10, 20)));

        props.MoveKeyFrame(5, 5);

        Assert.Single(props.Position.KeyFrames);
        Assert.Contains(props.Position.KeyFrames, k => k.Frame == 5 && k.Value.Equals(new LingoPoint(10, 20)));
        Assert.Contains(props.GetKeyFrames()!, k => k.Frame == 5);
    }

    [Fact]
    public void DeleteKeyFrame_RemovesData()
    {
        var props = new LingoSpriteAnimatorProperties();
        props.AddKeyFrame(new LingoKeyFrameSetting(5, position: new LingoPoint(10, 20)));

        var result = props.DeleteKeyFrame(5);

        Assert.True(result);
        Assert.Empty(props.Position.KeyFrames);
        Assert.Empty(props.GetKeyFrames()!);
    }
}
