using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BlingoEngine.Sprites;
using Xunit;

public class BlingoSprite2DManagerTests
{
    [Fact]
    public void RollOver_InactiveSprite_ReturnsFalse()
    {
        var manager = (BlingoSprite2DManager)FormatterServices.GetUninitializedObject(typeof(BlingoSprite2DManager));

        var field = typeof(BlingoSpriteManager<BlingoSprite2D>).GetField("_activeSprites", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(manager, new Dictionary<int, BlingoSprite2D>());

        var result = manager.RollOver(42);

        Assert.False(result);
    }
}

