using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using LingoEngine.Sprites;
using Xunit;

public class LingoSprite2DManagerTests
{
    [Fact]
    public void RollOver_InactiveSprite_ReturnsFalse()
    {
        var manager = (LingoSprite2DManager)FormatterServices.GetUninitializedObject(typeof(LingoSprite2DManager));

        var field = typeof(LingoSpriteManager<LingoSprite2D>).GetField("_activeSprites", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(manager, new Dictionary<int, LingoSprite2D>());

        var result = manager.RollOver(42);

        Assert.False(result);
    }
}
