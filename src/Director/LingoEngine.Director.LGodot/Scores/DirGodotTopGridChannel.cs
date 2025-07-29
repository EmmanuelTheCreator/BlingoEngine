using Godot;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Director.LGodot.Scores;

public interface IDirGodotTopGridChannel
{
    void SetMovie(LingoMovie? movie);
}


internal abstract partial class DirGodotTopGridChannelBase : Control, IDirGodotTopGridChannel
{
    protected LingoMovie? _movie;
    protected readonly DirScoreGfxValues _gfxValues;
    protected float _scrollX;
    protected bool _dirty = true;
    protected bool _spriteDirty = true;
    protected bool _spriteListDirty;
    protected int _lastFrame = -1;
    protected readonly IDirSpritesManager _spritesManager;



    protected DirGodotTopGridChannelBase(IDirSpritesManager spritesManager)
    {
        _spritesManager = spritesManager;
        _gfxValues = _spritesManager.GfxValues;
        MouseFilter = MouseFilterEnum.Stop;
        
    }

    protected void MarkDirty()
    {
        _dirty = true;
        QueueRedraw();
    }

    public virtual float ScrollX
    {
        get => _scrollX;
        set
        {
            _scrollX = value;
        }
    }

    public virtual void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            UnsubscribeMovie(_movie);
        _movie = movie;
        if (_movie != null)
        {
            SubscribeMovie(_movie);
            
        }
    }

    protected virtual void SubscribeMovie(LingoMovie movie) { }
    protected virtual void UnsubscribeMovie(LingoMovie movie) { }
    protected virtual bool HandleDrop(InputEventMouseButton mb) => false;

    public virtual bool HandleMouseEvent(LingoMouseEvent mouseEvent, int currentFrame)
    {
        return false;
    }
}






internal abstract partial class DirGodotTopGridChannel<TSpriteManager, TSpriteUI,TSprite> : DirGodotTopGridChannelBase, IDirGodotTopGridChannel, IDirGodotScoreDragableZone<TSprite>
    where TSpriteUI : DirGodotTopSprite<TSprite>
    where TSpriteManager : ILingoSpriteManager<TSprite>
    where TSprite : LingoSprite
{
    protected readonly List<TSpriteUI> _sprites = new();


    public int SpriteNum { get; }

    protected readonly IDirectorEventMediator _mediator;
    protected TSpriteManager? _manager;
    protected TSpriteUI? _dragSprite;
    private readonly SpritesCanvas _canvas;
    private readonly DirGodotScoreDragHandler<TSprite, TSpriteUI> _dragHandler;
    protected TSpriteUI? _selected;

    private bool _hasDirtySprites = true;
    bool IDirGodotScoreDragableZone<TSprite>.HasDirtySprites { get => _hasDirtySprites; set => _hasDirtySprites = value; }
    internal bool SpriteListDirty { get => _spriteListDirty; set => _spriteListDirty = value; }
    internal Rect2? SpritePreviewRect => _dragHandler.SpritePreviewRect;
    internal bool ShowPreview => _dragHandler.ShowPreview;
    internal int PreviewChannel => _dragHandler.PreviewChannel;

    internal int PreviewBegin => _dragHandler.PreviewBegin;
    internal int PreviewEnd => _dragHandler.PreviewEnd;

    protected readonly HashSet<Vector2I> _selectedCells = new();
    protected Vector2I? _lastSelectedCell = null;
    protected int _dragFrame;

    public override float ScrollX
    {
        set => base.ScrollX = value;
        get
        {
            _canvas.QueueRedraw();
            return base.ScrollX;
        }
    }

    protected DirGodotTopGridChannel(int spriteNum, IDirSpritesManager spritesManager)
        :base(spritesManager)
    {
        
        SpriteNum = spriteNum;
        _mediator = spritesManager.Mediator;
        _canvas = new SpritesCanvas(this, spritesManager);
        AddChild(_canvas);
        MouseFilter = MouseFilterEnum.Stop;
        _dragHandler = new DirGodotScoreDragHandler<TSprite, TSpriteUI>(this, null, _gfxValues, _sprites, spritesManager.CommandManager);
    }


    public override void _Draw()
    {
        base._Draw();
        if (_hasDirtySprites)
            RedrawAllSprites();
    }

    protected virtual void MoveSprite(TSpriteUI sprite, int oldFrame, int newFrame)
    {
        if (_manager == null) return;
        _manager.MoveSprite(sprite.Sprite, newFrame);
    }

    protected override void SubscribeMovie(LingoMovie movie)
    {
        if (_manager == null) return;
        _manager.SpriteListChanged += OnSpritesChanged;
    }

    protected override void UnsubscribeMovie(LingoMovie movie)
    {
        if (_manager == null) return;
        _manager.SpriteListChanged -= OnSpritesChanged;
    }

    private void OnSpritesChanged()
    {
        if (_movie == null || _manager == null) return;
        RedrawAllSprites();
        
    }
    public override bool HandleMouseEvent(LingoMouseEvent mouseEvent, int currentFrame)
    {
        base.HandleMouseEvent(mouseEvent, currentFrame);
        return _canvas.HandleMouseEvent(mouseEvent, currentFrame);
    }

    protected abstract TSpriteUI CreateUISprite(TSprite sprite, IDirSpritesManager spritesManager);
    protected virtual void OnDoubleClick(int frame, TSpriteUI? sprite) { }
    protected virtual void OnSpriteClicked(TSpriteUI sprite)
    {
        _mediator.RaiseSpriteSelected(sprite.Sprite);
        //_mediator.RaiseMemberSelected(sprite.Clip.Sound);
    }
   
    public override void SetMovie(LingoMovie? movie)
    {
        base.SetMovie(movie);
        _movie = movie;
        _sprites.Clear();
        if (_movie != null)
        {
            if (_manager != null)
                _manager.SpriteListChanged -= SpriteListChanged;
            _manager = GetManager(_movie);
            _manager.SpriteListChanged += SpriteListChanged;
            _dragHandler.SetMovie(_movie);
            RedrawAllSprites();
        }
        UpdateSize();
    }

    private void SpriteListChanged()
    {
        _hasDirtySprites = true;
        MarkDirty();
    }

    private void RedrawAllSprites()
    {
        _sprites.Clear();
        if (_manager == null) return;
        var sprites = _manager.GetAllSpritesByChannel(SpriteNum);
        foreach (var s in sprites)
        {
            var uiSprite = CreateUISprite(s, _spritesManager);
            _sprites.Add(uiSprite);
        }
        MarkDirty();
    }

    protected abstract TSpriteManager GetManager(LingoMovie movie);
    //public override void _GuiInput(InputEvent @event)
    //{
    //    if (_movie == null) return;

    //    if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
    //    {
    //        if (mb.Pressed)
    //        {
    //            _mouseFrame = Mathf.RoundToInt((mb.Position.X + _scrollX - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1;
    //            //var sprite = _sprites.FirstOrDefault(k => frame >= k.BeginFrame && frame <= k.EndFrame);
    //            //if (mb.DoubleClick)
    //            //{
    //            //    OnDoubleClick(frame, sprite);
    //            //}
    //            //else if (sprite != null)
    //            //{
    //            //    foreach (var sp in _sprites)
    //            //        sp.Selected = sp == sprite;
    //            //    _dragSprite = sprite;
    //            //    _dragFrame = frame;
    //            //    OnSpriteClicked(sprite);
    //            //    MarkDirty();
    //            //}
    //        }
    //        else
    //        {
    //            //if (!HandleDrop(mb))
    //            //    _dragSprite = null;
    //        }
    //    }
    //    else if (@event is InputEventMouseMotion) // && _dragSprite != null)
    //    {
    //        float frameF = (GetLocalMousePosition().X + _scrollX - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
    //        _mouseFrame = Math.Clamp(Mathf.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
    //        //if (newFrame != _dragFrame)
    //        //{
    //        //    MoveSprite(_dragSprite, _dragFrame, newFrame);
    //        //    _dragSprite.MoveToFrame(newFrame);
    //        //    _dragFrame = newFrame;
    //        //    _dirty = true;
    //        //}
    //    }
    //}

    protected virtual void UpdateSize()
    {
        if (_movie == null) return;
        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth;
        Size = new Vector2(width, _gfxValues.ChannelHeight);
        CustomMinimumSize = Size;
        _canvas.UpdateSize(Size.X, Size.Y);
        //_canvas.QueueRedraw();
    }
    public void HandleSelection(Vector2I cell, bool ctrl, bool shift)
    {
        if (shift && _lastSelectedCell.HasValue)
        {
            SelectRange(_lastSelectedCell.Value, cell);
            _lastSelectedCell = cell;
        }
        else if (ctrl)
        {
            if (!_selectedCells.Add(cell))
                _selectedCells.Remove(cell);
            _lastSelectedCell = cell;
        }
        else
        {
            _selectedCells.Clear();
            _selectedCells.Add(cell);
            _lastSelectedCell = cell;
        }

        //_spriteCanvas.QueueRedraw();
    }
    private void SelectRange(Vector2I from, Vector2I to)
    {
        _selectedCells.Clear();
        int minX = Mathf.Min(from.X, to.X);
        int maxX = Mathf.Max(from.X, to.X);
        int minY = Mathf.Min(from.Y, to.Y);
        int maxY = Mathf.Max(from.Y, to.Y);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                _selectedCells.Add(new Vector2I(x, y));
    }
    public void SelectSprite(DirGodotBaseSprite<TSprite>? sprite, bool raiseEvent = true)
       => SelectSprite(sprite as TSpriteUI, raiseEvent);
    public void SelectSprite(TSpriteUI? sprite, bool raiseEvent = true)
    {
        if (_selected == sprite) return;
        if (_selected != null) _selected.Selected = false;
        _selected = sprite;
        if (_selected != null)
        {
            _selected.Selected = true;
            if (raiseEvent)
                _mediator.RaiseSpriteSelected(_selected.Sprite);
        }
        _hasDirtySprites = true;
    }
    public bool IsCellSelected(Vector2I cell) => _selectedCells.Contains(cell);

    public bool IsSpriteSelected(TSpriteUI sprite)
    {
        if (_selectedCells.Count == 0)
            return _selected == sprite;

        int row = sprite.Sprite.SpriteNum - 1;
        for (int frame = sprite.Sprite.BeginFrame; frame <= sprite.Sprite.EndFrame; frame++)
        {
            if (_selectedCells.Contains(new Vector2I(frame - 1, row)))
                return true;
        }
        return false;
    }

    public void ClearSelection()
    {
        _selectedCells.Clear();
        _lastSelectedCell = null;
        //_spriteCanvas.QueueRedraw();
    }
    public Vector2I? GetCellFromPosition(Vector2 position)
    {
        if (_movie == null) return null;
        // Replace with your actual cell size logic
        const int CellWidth = 32;
        const int CellHeight = 32;

        int column = (int)(position.X / CellWidth);
        int row = (int)(position.Y / CellHeight);

        if (column < 0 || column >= _movie.MaxSpriteChannelCount || row < 0 || row >= _movie.FrameCount)
            return null;

        return new Vector2I(column, row);
    }

    void IDirGodotScoreDragableZone<TSprite>.SpriteCanvasQueueRedraw()
    {
        //_spriteCanvas.QueueRedraw();
    }

    private partial class SpritesCanvas : Control
    {
        private readonly DirGodotTopGridChannel<TSpriteManager, TSpriteUI, TSprite> _owner;
        private readonly LingoGfxCanvas _canvas;

        public SpritesCanvas(DirGodotTopGridChannel<TSpriteManager, TSpriteUI, TSprite> owner, IDirSpritesManager spritesManager)
        {
            _owner = owner;
            _canvas = spritesManager.Factory.CreateGfxCanvas("SpritesCanvas",30, owner._gfxValues.ChannelHeight);
            AddChild(_canvas.FrameworkObj as Node);
        }
        internal void UpdateSize(float width, float height)
        {
            _canvas.Width = width;
            Size = new Vector2(width, height);
            CustomMinimumSize = Size;
        }
        private TSpriteUI? _lastSprite = null;
        public bool HandleMouseEvent(LingoMouseEvent mouseEvent, int mouseFrame)
        {
            if (_owner._sprites.Count == 0) return false;
            var sprite = _owner._sprites.FirstOrDefault(x => x.Sprite.BeginFrame <= mouseFrame && mouseFrame <= x.Sprite.EndFrame);
            if (sprite == null)
            {
                if (_lastSprite == null)
                    return false;
                if (mouseEvent.Type == LingoMouseEventType.MouseUp)
                {
                    _lastSprite.SpriteUI.IsSelected = false;
                    _lastSprite = null;
                    return false;
                }
                sprite = _lastSprite;
            }
            //Console.WriteLine("Handle Sprite:"+ mouseFrame+"\t:"+ sprite.Sprite.Name+"\t:"+ sprite.Sprite.BeginFrame+"->"+ sprite.Sprite.EndFrame);
            sprite.SpriteUI.HandleMouse(mouseEvent, mouseFrame);
            _lastSprite = sprite;

            if (sprite.SpriteUI.RequireToRedraw)
                QueueRedraw();

            return true;
        }
        public override void _Draw()
        {
            _canvas.Clear(LingoColorList.Transparent); 
            
            if (_owner.SpritePreviewRect.HasValue)
            {
                var rect = _owner.SpritePreviewRect.Value;
                DrawRect(rect, new Color(1, 1, 1, 0.25f), filled: true);
                DrawRect(rect, new Color(1, 1, 1, 1), filled: false, width: 1);
            }

            var movie = _owner._movie;
            if (movie == null) return;

            int channelCount = movie.MaxSpriteChannelCount;
            var font = ThemeDB.FallbackFont;

            foreach (var sp in _owner._sprites)
            {
                sp.SpriteUI.Draw(_canvas, _owner._gfxValues.FrameWidth, _owner._gfxValues.ChannelHeight);
            }

            int cur = movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            float barX = _owner._gfxValues.LeftMargin + cur * _owner._gfxValues.FrameWidth + _owner._gfxValues.FrameWidth / 2f;
            DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * _owner._gfxValues.ChannelHeight), Colors.Red, 2);

            if (_owner.ShowPreview)
            {
                float px = _owner._gfxValues.LeftMargin + (_owner.PreviewBegin - 1) * _owner._gfxValues.FrameWidth;
                float pw = (_owner.PreviewEnd - _owner.PreviewBegin + 1) * _owner._gfxValues.FrameWidth;
                float py = _owner.PreviewChannel * _owner._gfxValues.ChannelHeight;
                DrawRect(new Rect2(px, py, pw, _owner._gfxValues.ChannelHeight), new Color(0, 0, 1, 0.3f));
            }

        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                var clickPos = mouseEvent.Position;
                var cell = _owner.GetCellFromPosition(clickPos);

                if (cell != null)
                {
                    var ctrl = Input.IsKeyPressed(Key.Ctrl);
                    var shift = Input.IsKeyPressed(Key.Shift);

                    _owner.HandleSelection(cell.Value, ctrl, shift);
                }
            }
        }

        
    }
}
