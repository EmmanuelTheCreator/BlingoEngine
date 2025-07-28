using Godot;
using System.Linq;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Container holding a stack of grid channels.
/// </summary>
internal partial class DirGodotGridChannelStack : Control, IDirScrollX, IDirMovieNode
{
    private readonly Control[] _channels;
    private readonly DirScoreGfxValues _gfxValues;

    public DirGodotGridChannelStack(DirScoreGfxValues gfxValues, params Control[] channels)
    {
        _gfxValues = gfxValues;
        _channels = channels;
        for (int i = 0; i < _channels.Length; i++)
        {
            _channels[i].Position = new Vector2(0, i * _gfxValues.ChannelHeight);
            AddChild(_channels[i]);
        }
    }

    public float ScrollX
    {
        get
        {
            foreach (var ch in _channels)
                if (ch is IDirScrollX s)
                    return s.ScrollX;
            return 0;
        }
        set
        {
            foreach (var ch in _channels)
                if (ch is IDirScrollX s)
                    s.ScrollX = value;
        }
    }

    public void SetMovie(LingoMovie? movie)
    {
        foreach (var ch in _channels)
            if (ch is IDirMovieNode m)
                m.SetMovie(movie);
        UpdateSize(movie);
    }

    private void UpdateSize(LingoMovie? movie)
    {
        if (movie == null) return;
        float width = movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        CustomMinimumSize = new Vector2(width, _gfxValues.ChannelHeight * _channels.Length);
        Size = CustomMinimumSize;
    }
}
