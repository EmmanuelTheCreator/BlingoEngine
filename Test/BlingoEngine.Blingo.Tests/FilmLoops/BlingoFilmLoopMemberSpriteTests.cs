using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Primitives;
using Moq;
using Xunit;

namespace BlingoEngine.Lingo.Tests.FilmLoops;

public class BlingoFilmLoopMemberSpriteTests
{
    [Fact]
    public void MemberSpriteDefaultsToMembersRegPoint()
    {
        var factory = new Mock<IBlingoFrameworkFactory>();
        var libs = new BlingoCastLibsContainer(factory.Object);
        var cast = (BlingoCast)libs.AddCast("Test");

        var bmpFramework = new Mock<IBlingoFrameworkMemberBitmap>();
        var bitmap = new BlingoMemberBitmap(cast, bmpFramework.Object, 1, "Bmp")
        {
            Width = 9,
            Height = 8,
            RegPoint = new APoint(4, 0)
        };

        var sprite = new BlingoFilmLoopMemberSprite(bitmap);

        Assert.Equal(bitmap.RegPoint, sprite.RegPoint);
    }
}

