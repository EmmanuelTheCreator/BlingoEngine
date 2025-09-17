using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Primitives;
using Moq;
using Xunit;

namespace BlingoEngine.Lingo.Tests.FilmLoops;

public class NestedFilmLoopBoundingBoxTests
{
    [Fact]
    public void OuterFilmLoopUsesInnerBoundsWhenChildSizeIsZero()
    {
        var factory = new Mock<IBlingoFrameworkFactory>();
        var libs = new BlingoCastLibsContainer(factory.Object);
        var cast = (BlingoCast)libs.AddCast("Test");

        var innerFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var inner = new BlingoFilmLoopMember(innerFramework.Object, cast, 1, "Inner");
        var innerSprite = new BlingoFilmLoopMemberSprite
        {
            LocH = 10,
            LocV = 10,
            Width = 20,
            Height = 20,
            BeginFrame = 1,
            EndFrame = 2
        };
        innerSprite.AddKeyframes((2, 20, 20));
        inner.AddSprite(innerSprite);

        var innerBounds = inner.GetBoundingBox();

        var outerFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var outer = new BlingoFilmLoopMember(outerFramework.Object, cast, 2, "Outer");
        var outerSprite = new BlingoFilmLoopMemberSprite(inner)
        {
            LocH = 0,
            LocV = 0,
            BeginFrame = 1,
            EndFrame = 2
        };
        outerSprite.AddKeyframes((2, 10, 10));
        outer.AddSprite(outerSprite);

        var bounds = outer.GetBoundingBox();

        Assert.Equal(innerBounds.Width + 10, bounds.Width);
        Assert.Equal(innerBounds.Height + 10, bounds.Height);
    }

    [Fact]
    public void GrandFilmLoopIncludesNegativeParentAnimation()
    {
        var factory = new Mock<IBlingoFrameworkFactory>();
        var libs = new BlingoCastLibsContainer(factory.Object);
        var cast = (BlingoCast)libs.AddCast("Test");

        // Inner film loop with sprite moving left
        var innerFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var inner = new BlingoFilmLoopMember(innerFramework.Object, cast, 1, "Inner");
        var innerSprite = new BlingoFilmLoopMemberSprite
        {
            LocH = 0,
            LocV = 0,
            Width = 20,
            Height = 20,
            BeginFrame = 1,
            EndFrame = 2
        };
        innerSprite.AddKeyframes((2, -10, 0));
        inner.AddSprite(innerSprite);

        // Outer film loop referencing inner
        var outerFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var outer = new BlingoFilmLoopMember(outerFramework.Object, cast, 2, "Outer");
        var outerSprite = new BlingoFilmLoopMemberSprite(inner)
        {
            LocH = 0,
            LocV = 0,
            BeginFrame = 1,
            EndFrame = 2
        };
        outer.AddSprite(outerSprite);

        // Grand film loop animating outer to a negative position
        var grandFramework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var grand = new BlingoFilmLoopMember(grandFramework.Object, cast, 3, "Grand");
        var grandSprite = new BlingoFilmLoopMemberSprite(outer)
        {
            LocH = 0,
            LocV = 0,
            BeginFrame = 1,
            EndFrame = 2
        };
        grandSprite.AddKeyframes((2, -10, 0));
        grand.AddSprite(grandSprite);

        var bounds = grand.GetBoundingBox();

        Assert.Equal(-10, bounds.Left);
        Assert.Equal(20, bounds.Width);
    }

    [Fact]
    public void UpdateSizeAccountsForSpritesLeftOfOrigin()
    {
        var factory = new Mock<IBlingoFrameworkFactory>();
        var libs = new BlingoCastLibsContainer(factory.Object);
        var cast = (BlingoCast)libs.AddCast("Test");

        var framework = new Mock<IBlingoFrameworkMemberFilmLoop>();
        var film = new BlingoFilmLoopMember(framework.Object, cast, 1, "Film");
        var sprite = new BlingoFilmLoopMemberSprite
        {
            LocH = -20,
            LocV = 0,
            Width = 10,
            Height = 10,
            BeginFrame = 1,
            EndFrame = 1
        };
        film.AddSprite(sprite);

        film.UpdateSize();

        Assert.Equal(25, film.Width);
        Assert.Equal(25, film.RegPoint.X);
    }
}

