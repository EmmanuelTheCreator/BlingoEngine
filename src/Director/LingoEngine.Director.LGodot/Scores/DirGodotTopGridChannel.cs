using Godot;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

internal abstract partial class DirGodotTopGridChannel<TSprite> : Control, IDirScrollX, IDirMovieNode where TSprite : DirGodotBaseSprite
{
    protected LingoMovie? _movie;
    protected readonly DirScoreGfxValues _gfxValues;
    protected readonly List<TSprite> _sprites = new();
    private readonly SpriteCanvas _canvas;
    private float _scrollX;
    private int _lastFrame = -1;
    protected bool _dirty = true;
    protected TSprite? _dragSprite;
    protected int _dragFrame;

    protected DirGodotTopGridChannel(DirScoreGfxValues gfxValues)
    {
        _gfxValues = gfxValues;
        _canvas = new SpriteCanvas(this);
        AddChild(_canvas);
        MouseFilter = MouseFilterEnum.Stop;
    }

    protected void MarkDirty() => _dirty = true;

    public float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = value;
            _canvas.QueueRedraw();
        }
    }

    public virtual void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            UnsubscribeMovie(_movie);
        _movie = movie;
        _sprites.Clear();
        if (_movie != null)
        {
            foreach (var s in BuildSprites(_movie))
                _sprites.Add(s);
            SubscribeMovie(_movie);
        }
        UpdateSize();
        _dirty = true;
    }

    protected virtual void SubscribeMovie(LingoMovie movie) { }
    protected virtual void UnsubscribeMovie(LingoMovie movie) { }

    protected abstract IEnumerable<TSprite> BuildSprites(LingoMovie movie);
    protected abstract void MoveSprite(TSprite sprite, int oldFrame, int newFrame);
    protected abstract void OnDoubleClick(int frame, TSprite? sprite);
    protected virtual void OnSpriteClicked(TSprite sprite) { }
    protected virtual bool HandleDrop(InputEventMouseButton mb) => false;

    public override void _GuiInput(InputEvent @event)
    {
        if (_movie == null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                int frame = Mathf.RoundToInt((mb.Position.X + _scrollX - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1;
                var sprite = _sprites.FirstOrDefault(k => frame >= k.BeginFrame && frame <= k.EndFrame);
                if (mb.DoubleClick)
                {
                    OnDoubleClick(frame, sprite);
                }
                else if (sprite != null)
                {
                    foreach (var sp in _sprites)
                        sp.Selected = sp == sprite;
                    _dragSprite = sprite;
                    _dragFrame = frame;
                    OnSpriteClicked(sprite);
                    MarkDirty();
                }
            }
            else
            {
                if (!HandleDrop(mb))
                    _dragSprite = null;
            }
        }
        else if (@event is InputEventMouseMotion && _dragSprite != null)
        {
            float frameF = (GetLocalMousePosition().X + _scrollX - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
            int newFrame = Math.Clamp(Mathf.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
            if (newFrame != _dragFrame)
            {
                MoveSprite(_dragSprite, _dragFrame, newFrame);
                _dragSprite.MoveToFrame(newFrame);
                _dragFrame = newFrame;
                _dirty = true;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_movie == null) return;
        int cur = _movie.CurrentFrame;
        if (_dirty || cur != _lastFrame)
        {
            _dirty = false;
            _lastFrame = cur;
            _canvas.QueueRedraw();
        }
    }

    protected virtual void UpdateSize()
    {
        if (_movie == null) return;
        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
        Size = new Vector2(width, _gfxValues.ChannelHeight);
        CustomMinimumSize = Size;
        _canvas.QueueRedraw();
    }

    private partial class SpriteCanvas : Control
    {
        private readonly DirGodotTopGridChannel<TSprite> _owner;
        public SpriteCanvas(DirGodotTopGridChannel<TSprite> owner) => _owner = owner;
        public override void _Draw()
        {
            var movie = _owner._movie;
            if (movie == null) return;
            var font = ThemeDB.FallbackFont;
            foreach (var sp in _owner._sprites)
            {
                float x = -_owner._scrollX + _owner._gfxValues.LeftMargin + (sp.BeginFrame - 1) * _owner._gfxValues.FrameWidth;
                float width = (sp.EndFrame - sp.BeginFrame + 1) * _owner._gfxValues.FrameWidth;
                sp.Draw(this, new Vector2(x, 0), width, _owner._gfxValues.ChannelHeight, font);
            }
            int cur = movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            float barX = -_owner._scrollX + _owner._gfxValues.LeftMargin + cur * _owner._gfxValues.FrameWidth + _owner._gfxValues.FrameWidth / 2f;
            DrawLine(new Vector2(barX, 0), new Vector2(barX, _owner._gfxValues.ChannelHeight), Colors.Red, 2);
        }
    }
}
