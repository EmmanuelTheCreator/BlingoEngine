using AbstUI.Primitives;
using LingoEngine.Movies;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal sealed class FakeFrameworkMovie : ILingoFrameworkMovie
{
    public string Name { get; set; } = "TestFrameworkMovie";
    public bool Visibility { get; set; } = true;
    public float Width { get; set; } = 640f;
    public float Height { get; set; } = 480f;
    public AMargin Margin { get; set; } = AMargin.Zero;
    public int ZIndex { get; set; }
    public object FrameworkNode => this;

    public void Dispose()
    {
    }

    public APoint GetGlobalMousePosition() => (0, 0);

    public void RemoveMe()
    {
    }

    public void UpdateStage()
    {
    }
}
