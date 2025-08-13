using LingoEngine.Bitmaps;
using LingoEngine.Casts;
using LingoEngine.FilmLoops;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using Moq;
using Xunit;

namespace LingoEngine.Lingo.Tests.FilmLoops;

public class NestedFilmLoopBoundingBoxTests
{
    [Fact]
    public void OuterFilmLoopUsesInnerBoundsWhenChildSizeIsZero()
    {
        var factory = new Mock<ILingoFrameworkFactory>();
        var libs = new LingoCastLibsContainer(factory.Object);
        var cast = (LingoCast)libs.AddCast("Test");

        var innerFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var inner = new LingoFilmLoopMember(innerFramework.Object, cast, 1, "Inner");
        var innerSprite = new LingoFilmLoopMemberSprite
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

        var outerFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var outer = new LingoFilmLoopMember(outerFramework.Object, cast, 2, "Outer");
        var outerSprite = new LingoFilmLoopMemberSprite(inner)
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
        var factory = new Mock<ILingoFrameworkFactory>();
        var libs = new LingoCastLibsContainer(factory.Object);
        var cast = (LingoCast)libs.AddCast("Test");

        // Inner film loop with sprite moving left
        var innerFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var inner = new LingoFilmLoopMember(innerFramework.Object, cast, 1, "Inner");
        var innerSprite = new LingoFilmLoopMemberSprite
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
        var outerFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var outer = new LingoFilmLoopMember(outerFramework.Object, cast, 2, "Outer");
        var outerSprite = new LingoFilmLoopMemberSprite(inner)
        {
            LocH = 0,
            LocV = 0,
            BeginFrame = 1,
            EndFrame = 2
        };
        outer.AddSprite(outerSprite);

        // Grand film loop animating outer to a negative position
        var grandFramework = new Mock<ILingoFrameworkMemberFilmLoop>();
        var grand = new LingoFilmLoopMember(grandFramework.Object, cast, 3, "Grand");
        var grandSprite = new LingoFilmLoopMemberSprite(outer)
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
}
