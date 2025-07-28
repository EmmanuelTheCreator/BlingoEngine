using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal abstract partial class DirGodotTopChannelHeader : Control, IDirMovieNode
{
    protected readonly DirScoreGfxValues _gfxValues;
    protected LingoMovie? _movie;

    protected DirGodotTopChannelHeader(DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        Size = new Vector2(_gfxValues.ChannelInfoWidth, _gfxValues.ChannelHeight);
        CustomMinimumSize = Size;
    }

    protected abstract string Icon { get; }
    protected virtual string Name => string.Empty;

    public virtual void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color("#f0f0f0"));
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(4, font.GetAscent() - 6), Icon,
            HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        if (!string.IsNullOrEmpty(Name))
        {
            DrawString(font, new Vector2(_gfxValues.ChannelHeight + 2, font.GetAscent() - 6), Name,
                HorizontalAlignment.Left, -1, 11, new Color("#a0a0a0"));
        }
    }
}
