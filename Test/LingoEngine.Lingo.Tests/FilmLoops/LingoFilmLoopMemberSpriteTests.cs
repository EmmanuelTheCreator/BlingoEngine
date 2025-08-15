using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.FilmLoops;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using Moq;
using Xunit;

namespace LingoEngine.Lingo.Tests.FilmLoops;

public class LingoFilmLoopMemberSpriteTests
{
    [Fact]
    public void MemberSpriteDefaultsToMembersRegPoint()
    {
        var factory = new Mock<ILingoFrameworkFactory>();
        var libs = new LingoCastLibsContainer(factory.Object);
        var cast = (LingoCast)libs.AddCast("Test");

        var bmpFramework = new Mock<ILingoFrameworkMemberBitmap>();
        var bitmap = new LingoMemberBitmap(cast, bmpFramework.Object, 1, "Bmp")
        {
            Width = 9,
            Height = 8,
            RegPoint = new APoint(4, 0)
        };

        var sprite = new LingoFilmLoopMemberSprite(bitmap);

        Assert.Equal(bitmap.RegPoint, sprite.RegPoint);
    }
}
