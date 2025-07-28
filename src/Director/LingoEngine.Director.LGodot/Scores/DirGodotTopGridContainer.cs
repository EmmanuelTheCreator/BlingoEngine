using Godot;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Commands;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTopGridContainer : Control
{
    private readonly DirScoreGfxValues _gfxValues;
    private readonly DirScoreGridPainter _gridCanvas;
    private readonly SubViewport _viewport = new();
    private readonly TextureRect _texture = new();
    private readonly Control _clipper = new();
    private readonly DirGodotTopGridChannelBase[] _channels;
    private bool _collapsed;
    private LingoMovie? _movie;
    private float _scrollX;
    public bool Collapsed
    {
        get => _collapsed;
        set
        {
            _collapsed = value;
            Visible = !value;
            QueueRedraw();
        }
    }
    public DirGodotTopGridContainer(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory, IDirectorEventMediator mediator, ILingoCommandManager commandManager)
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

        _channels =
        [
            new DirGodotTempoGridChannel(gfxValues,mediator,factory,commandManager),
            new DirGodotColorPaletteGridChannel(gfxValues,mediator,factory,commandManager),
            new DirGodotTransitionGridChannel(gfxValues,mediator,factory,commandManager),
            new DirGodotAudioGridChannel(1, gfxValues, mediator,factory,commandManager),
            new DirGodotAudioGridChannel(2, gfxValues, mediator,factory,commandManager)
        ];

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
            //_gridCanvas.ScrollX = value;
            foreach (var ch in _channels)
                ch.ScrollX = value;
            Position = new Vector2(-value, Position.Y);
            QueueRedraw();
        }
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        foreach (var ch in _channels)
            ch.SetMovie(movie);
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (_movie == null)
            return;
        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
        float height = _channels.Length * _gfxValues.ChannelHeight;
        CustomMinimumSize = Size;
        Size = new Vector2(width, height);
        _clipper.CustomMinimumSize = Size;
        _clipper.Size = Size;
        _viewport.SetSize(new Vector2I((int)width, (int)height));
        _texture.CustomMinimumSize = new Vector2(width, height);
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = _channels.Length;
        _gridCanvas.Draw();
        foreach (var ch in _channels)
            if (ch is Control c)
                c.CustomMinimumSize = new Vector2(width, _gfxValues.ChannelHeight);
    }
    private int _lastDrawnFrame = 0;
    public override void _Draw()
    {
        base._Draw();
        if (_movie == null) return;
            _lastDrawnFrame = _movie.CurrentFrame;
        int cur = _movie.CurrentFrame - 1;
        if (cur < 0) cur = 0;
        float barX = - _scrollX + _gfxValues.LeftMargin + cur * _gfxValues.FrameWidth + _gfxValues.FrameWidth / 2f;
        DrawLine(new Vector2(barX, 0), new Vector2(barX, Size.Y), Colors.Red, 2);
    }
}
