using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Director.Core.Tools;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Draws the shared background grid for the collapsible top channels.
/// </summary>
internal partial class DirGodotTopGridContainer : Control, IDirScrollX, IDirMovieNode
{
    private readonly DirScoreGfxValues _gfxValues;
    private readonly DirScoreGridPainter _gridCanvas;
    private readonly SubViewport _viewport = new();
    private readonly TextureRect _texture = new();
    private readonly Control _clipper = new();
    private readonly Control[] _channels;
    private LingoMovie? _movie;
    private float _scrollX;

    public DirGodotTopGridContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, IDirectorEventMediator mediator)
    {
        _gfxValues = gfxValues;
        _gridCanvas = new DirScoreGridPainter(factory, gfxValues);

        _viewport.SetDisable3D(true);
        _viewport.TransparentBg = true;
        _viewport.SetUpdateMode(SubViewport.UpdateMode.Once);
        _viewport.AddChild(_gridCanvas.Canvas.Framework<LingoGodotGfxCanvas>());
        _texture.Texture = _viewport.GetTexture();
        _texture.MouseFilter = MouseFilterEnum.Ignore;

        _clipper.ClipContents = true;
        _clipper.AddChild(_viewport);
        _clipper.AddChild(_texture);
        AddChild(_clipper);

        _channels = new Control[]
        {
            new DirGodotTempoGridChannel(gfxValues),
            new DirGodotColorPaletteGridChannel(gfxValues),
            new DirGodotTransitionGridChannel(gfxValues),
            new DirGodotSoundGridChannel(0, gfxValues, mediator),
            new DirGodotSoundGridChannel(1, gfxValues, mediator)
        };

        for (int i = 0; i < _channels.Length; i++)
        {
            _channels[i].Position = new Vector2(0, i * _gfxValues.ChannelHeight);
            _clipper.AddChild(_channels[i]);
        }

        UpdateSize();
    }

    public float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = value;
            _gridCanvas.ScrollX = value;
            foreach (var ch in _channels)
                if (ch is IDirScrollX s) s.ScrollX = value;
            QueueRedraw();
        }
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        foreach (var ch in _channels)
            if (ch is IDirMovieNode m) m.SetMovie(movie);
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (_movie == null)
            return;
        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
        float height = _channels.Length * _gfxValues.ChannelHeight;
        Size = new Vector2(width, height);
        CustomMinimumSize = Size;
        _viewport.SetSize(new Vector2I((int)width, (int)height));
        _texture.CustomMinimumSize = new Vector2(width, height);
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = _channels.Length;
        _gridCanvas.Draw();
        foreach (var ch in _channels)
            if (ch is Control c)
                c.CustomMinimumSize = new Vector2(width, _gfxValues.ChannelHeight);
    }
}
