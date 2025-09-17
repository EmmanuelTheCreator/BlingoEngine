using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Events;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using Moq;
using Xunit;

namespace BlingoEngine.Lingo.Tests.FilmLoops;

public class FilmLoopComposerTests
{
    [Fact]
    public void PrepareUsesRegistrationPointForTransform()
    {
        var factory = new Mock<IBlingoFrameworkFactory>();
        var libs = new BlingoCastLibsContainer(factory.Object);
        var cast = (BlingoCast)libs.AddCast("Test");

        var bmpFramework = new Mock<IBlingoFrameworkMemberBitmap>();
        var bitmap = new BlingoMemberBitmap(cast, bmpFramework.Object, 1, "Bmp")
        {
            Width = 20,
            Height = 20,
            RegPoint = new APoint(0, 0)
        };

        var filmFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var film = new BlingoFilmLoopMember(filmFramework.Object, cast, 2, "Film");
        var entry = new BlingoFilmLoopMemberSprite(bitmap)
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

        var mediator = new BlingoEventMediator();
        var spritesPlayer = new Mock<IBlingoSpritesPlayer>();
        var runtime = new BlingoSprite2DVirtual(mediator, spritesPlayer.Object, entry, libs);
        var layers = new List<BlingoSprite2DVirtual> { runtime };

        var prep = BlingoFilmLoopComposer.Prepare(film, BlingoFilmLoopFraming.Auto, layers);
        var layer = prep.Layers[0];
        var topLeft = layer.Transform.TransformPoint(new APoint(0, 0));

        Assert.Equal(0, topLeft.X);
        Assert.Equal(0, topLeft.Y);
    }
}

