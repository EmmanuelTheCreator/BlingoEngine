using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.Events;
using LingoEngine.FilmLoops;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using LingoEngine.Sprites;
using Moq;
using Xunit;

namespace LingoEngine.Lingo.Tests.FilmLoops;

public class FilmLoopComposerTests
{
    [Fact]
    public void PrepareUsesRegistrationPointForTransform()
    {
        var factory = new Mock<ILingoFrameworkFactory>();
        var libs = new LingoCastLibsContainer(factory.Object);
        var cast = (LingoCast)libs.AddCast("Test");

        var bmpFramework = new Mock<ILingoFrameworkMemberBitmap>();
        var bitmap = new LingoMemberBitmap(cast, bmpFramework.Object, 1, "Bmp")
        {
            Width = 20,
            Height = 20,
            RegPoint = new APoint(0, 0)
        };

        var filmFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var film = new LingoFilmLoopMember(filmFramework.Object, cast, 2, "Film");
        var entry = new LingoFilmLoopMemberSprite(bitmap)
        {
            LocH = 0,
            LocV = 0,
            Width = 20,
            Height = 20,
            BeginFrame = 1,
            EndFrame = 1
        };
        film.AddSprite(entry);
        film.UpdateSize();

        var mediator = new LingoEventMediator();
        var spritesPlayer = new Mock<ILingoSpritesPlayer>();
        var runtime = new LingoSprite2DVirtual(mediator, spritesPlayer.Object, entry, libs);
        var layers = new List<LingoSprite2DVirtual> { runtime };

        var prep = LingoFilmLoopComposer.Prepare(film, LingoFilmLoopFraming.Auto, layers);
        var layer = prep.Layers[0];
        var topLeft = layer.Transform.TransformPoint(new APoint(0, 0));

        Assert.Equal(0, topLeft.X);
        Assert.Equal(0, topLeft.Y);
    }
}
