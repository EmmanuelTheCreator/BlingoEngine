﻿using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreGrid : Control, IHasSpriteSelectedEvent, IDirGodotScoreDragableZone<LingoSprite2D>
{
    private LingoMovie? _movie;

    

    private readonly List<DirGodotScoreSprite> _sprites = new();
    private DirGodotScoreSprite? _selected;
    /// <summary>
    /// Currently selected sprite, if any.
    /// </summary>
    internal LingoSprite2D? SelectedSprite => _selected?.Sprite;
    private readonly IDirectorEventMediator _mediator;
    private readonly ILingoCommandManager _commandManager;
    private readonly PopupMenu _contextMenu = new();
    private DirGodotScoreSprite? _contextSprite;
    private readonly DirScoreGfxValues _gfxValues;

    private readonly SubViewport _gridViewport = new();
    private readonly SubViewport _spriteViewport = new();
    private readonly TextureRect _gridTexture = new();
    private readonly TextureRect _spriteTexture = new();
    private readonly SpriteCanvas _spriteCanvas;
    private readonly DirGodotScoreDragHandler<LingoSprite2D, DirGodotScoreSprite> _dragHandler;
    private bool _hasDirtySprites = true;
    private bool _spriteListDirty;
    private int _lastFrame = -1;
    internal bool SpriteListDirty { get  => _spriteListDirty; set => _spriteListDirty = value; }
    bool IDirGodotScoreDragableZone<LingoSprite2D>.HasDirtySprites { get => _hasDirtySprites; set => _hasDirtySprites = value; }
    internal Rect2? SpritePreviewRect => _dragHandler.SpritePreviewRect;
    internal bool ShowPreview => _dragHandler.ShowPreview;
    internal int PreviewChannel => _dragHandler.PreviewChannel;

    internal int PreviewBegin => _dragHandler.PreviewBegin;
    internal int PreviewEnd => _dragHandler.PreviewEnd;

    private readonly HashSet<Vector2I> _selectedCells = new();
    private Vector2I? _lastSelectedCell = null;


    private readonly ILingoFrameworkFactory _factory;
    private readonly DirScoreGridPainter _gridCanvas;

    public DirGodotScoreGrid(IDirectorEventMediator mediator, DirScoreGfxValues gfxValues, ILingoCommandManager commandManager, IHistoryManager historyManager, ILingoFrameworkFactory factory)
    {
        _gfxValues = gfxValues;
        _mediator = mediator;
        _commandManager = commandManager;
        _factory = factory;
        AddChild(_contextMenu);
        _contextMenu.IdPressed += OnContextMenuItem;

        _gridViewport.SetDisable3D(true);
        _gridViewport.TransparentBg = true;
        _gridViewport.SetUpdateMode(SubViewport.UpdateMode.Once);
        _gridCanvas = new DirScoreGridPainter(_factory, _gfxValues);
        _gridViewport.AddChild(_gridCanvas.Canvas.Framework<LingoGodotGfxCanvas>());

        
        _spriteViewport.SetDisable3D(true);
        _spriteViewport.TransparentBg = true;
        _spriteViewport.SetUpdateMode(SubViewport.UpdateMode.Once);
        _spriteCanvas = new SpriteCanvas(this);
        _spriteViewport.AddChild(_spriteCanvas);

        _gridTexture.Texture = _gridViewport.GetTexture();
        _gridTexture.Position = Vector2.Zero;
        // Ensure textures draw above the window background
        //_gridTexture.ZIndex = 0;
        _gridTexture.MouseFilter = MouseFilterEnum.Ignore;
        
        MouseFilter = MouseFilterEnum.Stop;

        _spriteTexture.Texture = _spriteViewport.GetTexture();
        _spriteTexture.Position = Vector2.Zero;
        //_spriteTexture.ZIndex = 1;
        _spriteTexture.MouseFilter = MouseFilterEnum.Ignore;

        AddChild(_gridViewport);
        AddChild(_spriteViewport);
        AddChild(_gridTexture);
        AddChild(_spriteTexture);
        _dragHandler = new DirGodotScoreDragHandler<LingoSprite2D, DirGodotScoreSprite>(this,_movie, _gfxValues,_sprites, _commandManager);
    }

    public void SetMovie(LingoMovie? movie)
    {
        if (_movie != null)
            _movie.SpriteListChanged -= OnSpritesChanged;

        _movie = movie;
        BuildSpriteList();
        _spriteListDirty = false;
        if (_movie != null)
        {
            _movie.SpriteListChanged += OnSpritesChanged;
            _gridCanvas.FrameCount = _movie.FrameCount;
            _gridCanvas.ChannelCount = _movie.MaxSpriteChannelCount;
            _dragHandler.SetMovie(_movie);
            _gridCanvas.Draw();
        }

        UpdateViewportSize();
        _hasDirtySprites = true;
    }

    private void OnSpritesChanged()
    {
        _spriteListDirty = true;
        _hasDirtySprites = true;
        RefreshSprites();
    }

    private void BuildSpriteList()
    {
        _sprites.Clear();
        if (_movie != null)
        {
            int idx = 1;
            while (_movie.TryGetAllTimeSprite(idx, out var sp))
            {
                _sprites.Add(new DirGodotScoreSprite(sp));
                idx++;
            }
        }
    }

    private void RefreshSprites()
    {
        
        if (_spriteListDirty)
        {
            ForceRefreshSpriteViewPort();
            BuildSpriteList();
            UpdateViewportSize();
            _spriteListDirty = false;
            _hasDirtySprites = true;
        }
    }

    private void ForceRefreshSpriteViewPort()
    {
        _spriteViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        _spriteViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
        _spriteViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
    }

    public void SelectSprite(DirGodotBaseSprite<LingoSprite2D>? sprite, bool raiseEvent = true)
        => SelectSprite(sprite as DirGodotScoreSprite, raiseEvent);
    public void SelectSprite(DirGodotScoreSprite? sprite, bool raiseEvent = true)
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


    public override void _Input(InputEvent @event)
    {
        if (!Visible || _movie == null) return;

        if (@event is InputEventMouseButton mb)
        {
            Vector2 pos = GetLocalMousePosition();
            int totalChannels = _movie!.MaxSpriteChannelCount;
            int channel = (int)(pos.Y / _gfxValues.ChannelHeight);
            _dragHandler.HandleMouseButton(mb, pos, channel);
            if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
                TryOpenContextMenu(pos, channel);
        }
        else if (@event is InputEventMouseMotion)
            _dragHandler.HandleMouseMotion();
        
    }
    void IDirGodotScoreDragableZone<LingoSprite2D>.SpriteCanvasQueueRedraw() => _spriteCanvas.QueueRedraw();
    internal void MarkSpriteDirty()
    {
        _hasDirtySprites = true;
        _spriteCanvas.QueueRedraw();
    }


    internal void TryOpenContextMenu(Vector2 pos, int channel)
    {
        var sp = GetSpriteAt(pos, channel);
        if (sp == null || sp.Sprite.Member == null) return;

        _contextSprite = sp;
        _contextMenu.Clear();
        _contextMenu.AddItem("Find Cast Member", 1);
        var gp = GetGlobalMousePosition();
        _contextMenu.Popup(new Rect2I((int)gp.X, (int)gp.Y, 0, 0));
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

        _spriteCanvas.QueueRedraw();
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

    public bool IsCellSelected(Vector2I cell) => _selectedCells.Contains(cell);

    public bool IsSpriteSelected(DirGodotScoreSprite sprite)
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
        _spriteCanvas.QueueRedraw();
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


    #region Old

    //public override bool _CanDropData(Vector2 atPosition, Variant data)
    //{
    //    GD.Print($"Score: _CanDropData called at {atPosition} with {data.Obj}");
    //    _showPreview = false;
    //    if (_movie == null) return false;

    //    if (data.Obj is not ILingoMember member) return false;

    //    if (member.Type == LingoMemberType.Sound) return false;

    //    int channel = (int)(atPosition.Y / _gfxValues.ChannelHeight);
    //    if (channel < 0 || channel >= _movie.MaxSpriteChannelCount) return false;

    //    int start = Math.Clamp(Mathf.RoundToInt((atPosition.X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth) + 1, 1, _movie.FrameCount);
    //    int end = _movie.GetNextLabelFrame(start) - 1;
    //    int nextSprite = _movie.GetNextSpriteStart(channel, start);
    //    if (nextSprite != -1)
    //        end = Math.Min(end, nextSprite - 1);
    //    if (_movie.GetPrevSpriteEnd(channel, start) >= start)
    //        return false;

    //    end = Math.Clamp(end, start, _movie.FrameCount);

    //    _previewChannel = channel;
    //    _previewBegin = start;
    //    _previewEnd = end;
    //    _previewMember = member;
    //    _showPreview = true;
    //    _spriteDirty = true;
    //    return true;
    //}

    //public override void _DropData(Vector2 atPosition, Variant data)
    //{
    //    GD.Print($"Score: _DropData called at {atPosition} with {_previewMember}");
    //    _showPreview = false;
    //    _spriteDirty = true;
    //    if (_movie == null) return;
    //    if (_previewMember == null) return;

    //    var sp = _movie.AddSprite(_previewChannel + 1, _previewBegin, _previewEnd, 0, 0, s =>
    //    {
    //        s.SetMember(_previewMember);
    //    });
    //    _previewMember = null;
    //    _spriteListDirty = true;
    //} 
    #endregion

    public override void _Process(double delta)
    {
        if (Visible)
            RefreshSprites();
        if (!Visible) return;
        int cur = _movie?.CurrentFrame ?? -1;
        if (_hasDirtySprites || cur != _lastFrame)
        {
            ForceRefreshSpriteViewPort();
            _hasDirtySprites = false;
            _lastFrame = cur;
            _spriteCanvas.QueueRedraw();
        }
        // When dragging from external controls (like the Cast window), mouse
        // motion events may be captured and never reach the score grid. Invoke
        // the drag handler each frame so previews update correctly.
        if (_movie != null)
            _dragHandler.HandleMouseMotion();
    }

    public void SpriteSelected(ILingoSpriteBase sprite)
    {
        if (!(sprite is ILingoSprite)) return;
        var match = _sprites.FirstOrDefault(x => x.Sprite == sprite);
        SelectSprite(match, false);
    }

    private DirGodotScoreSprite? GetSpriteAt(Vector2 pos, int channel)
    {
        foreach (var sp in _sprites)
        {
            int sc = sp.Sprite.SpriteNum - 1;
            if (sc == channel)
            {
                float sx = _gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _gfxValues.FrameWidth;
                float ex = _gfxValues.LeftMargin + sp.Sprite.EndFrame * _gfxValues.FrameWidth;
                if (pos.X >= sx && pos.X <= ex)
                    return sp;
            }
        }
        return null;
    }

    private void OnContextMenuItem(long id)
    {
        if (id == 1 && _contextSprite?.Sprite.Member != null)
        {
            _mediator.RaiseFindMember(_contextSprite.Sprite.Member);
        }
        _contextSprite = null;
    }

    private void UpdateViewportSize()
    {
        if (_movie == null) return;

        float width = _gfxValues.LeftMargin + _movie.FrameCount * _gfxValues.FrameWidth + _gfxValues.ExtraMargin;
        float height = _movie.MaxSpriteChannelCount * _gfxValues.ChannelHeight + _gfxValues.ExtraMargin;

        Size = new Vector2(width, height);
        CustomMinimumSize = Size;

        _gridViewport.SetSize(new Vector2I((int)width, (int)height));
        _spriteViewport.SetSize(new Vector2I((int)width, (int)height));
        _gridTexture.CustomMinimumSize = new Vector2(width, height);
        _spriteTexture.CustomMinimumSize = new Vector2(width, height);
        _gridCanvas.FrameCount = _movie.FrameCount;
        _gridCanvas.ChannelCount = _movie.MaxSpriteChannelCount;
        _gridCanvas.Draw();
        _spriteCanvas.QueueRedraw();
    }

  
}

