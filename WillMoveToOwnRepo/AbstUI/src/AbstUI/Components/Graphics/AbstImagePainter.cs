using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Components.Graphics;

public abstract class AbstImagePainter<TTexture> : IAbstImagePainter
{
    public readonly record struct DrawAction(bool NeedResize, APoint Position, APoint Size, ARect? SrcRect, ARect? DestRect, Action<TTexture, ARect?, ARect?> Execute);

    protected readonly List<DrawAction> _drawActions = new();
    private AColor? _clearColor;
    protected bool _dirty;
    protected readonly int _maxWidth;
    protected readonly int _maxHeight;
    private int _width;
    private int _height;

    // Grid rendering support
    protected readonly Dictionary<(int X, int Y), TTexture> _tiles = new();
    protected readonly Dictionary<(int X, int Y), List<DrawAction>> _tileActions = new();
    protected readonly HashSet<(int X, int Y)> _dirtyTiles = new();
    protected int _offsetX;
    protected int _offsetY;

    protected AbstImagePainter(int width, int height, int maxWidth, int maxHeight)
    {
        _maxWidth = maxWidth == 0 ? 2048 : maxWidth;
        _maxHeight = maxHeight == 0 ? 2048 : maxHeight;
        _width = width > 0 ? Math.Min(width, _maxWidth) : 10;
        _height = height > 0 ? Math.Min(height, _maxHeight) : 10;
        _dirty = true;
    }

    public int Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            MarkDirty();
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            MarkDirty();
        }
    }

    public bool Pixilated { get; set; }
    public bool AutoResizeWidth { get; set; } = false;
    public bool AutoResizeHeight { get; set; } = true;
    public string Name { get; set; } = string.Empty;

    public bool UseTextureGrid { get; set; }
    public int TileSize { get; set; } = 128;

    protected int OffsetX => _offsetX;
    protected int OffsetY => _offsetY;

    protected void AddDrawAction(DrawAction action)
    {
        _drawActions.Add(action);
        _dirty = true;
        if (!UseTextureGrid) return;
#if NET48
        int startX = Math.Max(0, (int)Math.Floor((action.Position.X / TileSize)));
        int startY = Math.Max(0, (int)Math.Floor((action.Position.Y / TileSize)));
        int endX = Math.Max(0, (int)Math.Floor((action.Position.X + action.Size.X - 1) / TileSize));
        int endY = Math.Max(0, (int)Math.Floor((action.Position.Y + action.Size.Y - 1) / TileSize));
#else
        int startX = Math.Max(0, (int)MathF.Floor(action.Position.X / TileSize));
        int startY = Math.Max(0, (int)MathF.Floor(action.Position.Y / TileSize));
        int endX = Math.Max(0, (int)MathF.Floor((action.Position.X + action.Size.X - 1) / TileSize));
        int endY = Math.Max(0, (int)MathF.Floor((action.Position.Y + action.Size.Y - 1) / TileSize));
#endif
        for (int x = startX; x <= endX; x++)
            for (int y = startY; y <= endY; y++)
                AddTileAction((x, y), action);
    }

    protected void AddTileAction((int X, int Y) tile, DrawAction action)
    {
        if (!_tileActions.TryGetValue(tile, out var list))
        {
            list = new List<DrawAction>();
            _tileActions[tile] = list;
        }
        list.Add(action);
        _dirtyTiles.Add(tile);
        _dirty = true;
    }

    protected void MarkDirty()
    {
        _dirty = true;
        if (UseTextureGrid)
        {
            int tilesX = (Width + TileSize - 1) / TileSize;
            int tilesY = (Height + TileSize - 1) / TileSize;
            for (int x = 0; x < tilesX; x++)
                for (int y = 0; y < tilesY; y++)
                    _dirtyTiles.Add((x, y));
        }
    }

    protected void MarkDirty(APoint position, APoint size)
    {
        _dirty = true;
        if (!UseTextureGrid) return;
#if NET48
       int startX = Math.Max(0, (int)Math.Floor(position.X / TileSize));
        int startY = Math.Max(0, (int)Math.Floor(position.Y / TileSize));
        int endX = Math.Max(0, (int)Math.Floor((position.X + size.X - 1) / TileSize));
        int endY = Math.Max(0, (int)Math.Floor((position.Y + size.Y - 1) / TileSize));
#else
        int startX = Math.Max(0, (int)MathF.Floor(position.X / TileSize));
        int startY = Math.Max(0, (int)MathF.Floor(position.Y / TileSize));
        int endX = Math.Max(0, (int)MathF.Floor((position.X + size.X - 1) / TileSize));
        int endY = Math.Max(0, (int)MathF.Floor((position.Y + size.Y - 1) / TileSize));
#endif
        for (int x = startX; x <= endX; x++)
            for (int y = startY; y <= endY; y++)
                _dirtyTiles.Add((x, y));
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _tileActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    public void Resize(int width, int height)
    {
        width = Math.Min(width, _maxWidth);
        height = Math.Min(height, _maxHeight);
        if (_width == width && _height == height) return;
        _width = width;
        _height = height;
        MarkDirty();
    }

    public void Render()
    {
        var newWidth = Width;
        var newHeight = Height;
        if (AutoResizeWidth || AutoResizeHeight)
        {
            foreach (var action in _drawActions)
            {
                if (!action.NeedResize) continue;
                var candidateW = (int)MathF.Ceiling(action.Position.X + action.Size.X);
                var candidateH = (int)MathF.Ceiling(action.Position.Y + action.Size.Y);
                if (AutoResizeWidth && candidateW > newWidth)
                    newWidth = candidateW;
                if (AutoResizeHeight && candidateH > newHeight)
                    newHeight = candidateH;
            }
        }

        var targetWidth = AutoResizeWidth ? Math.Max(Width, newWidth) : Width;
        var targetHeight = AutoResizeHeight ? Math.Max(Height, newHeight) : Height;
        targetWidth = Math.Min(targetWidth, _maxWidth);
        targetHeight = Math.Min(targetHeight, _maxHeight);

        if (!UseTextureGrid)
        {
            if (!_dirty && targetWidth == _width && targetHeight == _height)
                return;

            if (targetWidth != Width || targetHeight != Height)
            {
                ResizeTexture(targetWidth, targetHeight);
                _width = targetWidth;
                _height = targetHeight;
            }

            BeginRender(_clearColor ?? AColor.FromRGBA(0, 0, 0, 0));
            foreach (var action in _drawActions)
                action.Execute(Target, action.SrcRect, action.DestRect);
            EndRender();
            _dirty = false;
            return;
        }

        if (_dirtyTiles.Count == 0 && targetWidth == _width && targetHeight == _height)
            return;

        _width = targetWidth;
        _height = targetHeight;
        var clearColor = _clearColor ?? AColor.FromRGBA(0, 0, 0, 0);
        foreach (var tilePos in _dirtyTiles)
        {
            int ox = tilePos.X * TileSize;
            int oy = tilePos.Y * TileSize;
            int tw = Math.Min(TileSize, _width - ox);
            int th = Math.Min(TileSize, _height - oy);
            if (!_tiles.TryGetValue(tilePos, out var tex) || tex == null || tex.Equals(default(TTexture)))
            {
                tex = CreateTileTexture(tw, th);
                _tiles[tilePos] = tex;
            }

            UseTexture(tex);
            _offsetX = ox;
            _offsetY = oy;
            BeginRender(clearColor);
            if (_tileActions.TryGetValue(tilePos, out var actions))
            {
                foreach (var action in actions)
                    action.Execute(tex, action.SrcRect, action.DestRect);
            }
            EndRender();
        }
        _offsetX = 0;
        _offsetY = 0;
        _dirtyTiles.Clear();
        _dirty = false;
    }

    protected abstract void BeginRender(AColor clearColor);
    protected abstract void EndRender();
    protected abstract void ResizeTexture(int width, int height);
    protected virtual TTexture CreateTileTexture(int width, int height) => default!;
    protected virtual void DestroyTileTexture(TTexture texture) { }
    protected virtual void UseTexture(TTexture texture) { }
    protected abstract TTexture Target { get; }

    protected void DisposeTiles()
    {
        foreach (var tex in _tiles.Values)
            DestroyTileTexture(tex);
        _tiles.Clear();
        _dirtyTiles.Clear();
    }

    public abstract void SetPixel(APoint point, AColor color);
    public abstract void DrawLine(APoint start, APoint end, AColor color, float width = 1);
    public abstract void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1);
    public abstract void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1);
    public abstract void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1);
    public abstract void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1);
    public abstract void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format);
    public abstract void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position);
    public abstract void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0);
    public abstract void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular, int letterSpacing = 0);
    public abstract IAbstTexture2D GetTexture(string? name = null);
    public abstract void Dispose();
}
