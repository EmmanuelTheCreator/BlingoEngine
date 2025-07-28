using Godot;
using System.Linq;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTopHeaderContainer : Control, IDirMovieNode, IDirCollapsibleHeader
{
    private readonly DirGodotTopChannelHeader[] _headers;
    private readonly DirScoreGfxValues _gfxValues;
    private bool _collapsed;

    public DirGodotTopHeaderContainer(DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        _headers = new DirGodotTopChannelHeader[]
        {
            new DirGodotTempoHeader(gfxValues),
            new DirGodotColorPaletteHeader(gfxValues),
            new DirGodotTransitionHeader(gfxValues),
            new DirGodotSoundHeader(0, gfxValues),
            new DirGodotSoundHeader(1, gfxValues)
        };
        foreach (var (h, idx) in _headers.Select((h,i)=>(h,i)))
        {
            h.Position = new Vector2(0, idx * _gfxValues.ChannelHeight);
            AddChild(h);
        }
        Size = new Vector2(_gfxValues.ChannelInfoWidth, _gfxValues.ChannelHeight * _headers.Length);
        CustomMinimumSize = Size;
    }

    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            _collapsed = value;
            Visible = true; // headers always visible
            foreach (var h in _headers)
                if (h is IDirCollapsibleHeader c) c.Collapsed = value;
        }
    }

    public void SetMovie(LingoMovie? movie)
    {
        foreach (var h in _headers) h.SetMovie(movie);
    }

    public bool IsMuted(int channel) => ((DirGodotSoundHeader)_headers[3 + channel]).IsMuted;
}
