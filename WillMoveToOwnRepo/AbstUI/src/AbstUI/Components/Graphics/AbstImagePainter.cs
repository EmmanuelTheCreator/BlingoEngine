using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Components.Graphics;

public abstract class AbstImagePainter<TTexture> : IAbstImagePainter
{
    public readonly record struct DrawAction(bool NeedResize, APoint Position, APoint Size, Action<TTexture> Execute);

    protected readonly List<DrawAction> _drawActions = new();
    private AColor? _clearColor;
    protected bool _dirty;
    protected readonly int _maxWidth;
    protected readonly int _maxHeight;
    private int _width;
    private int _height;

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

    protected void MarkDirty() => _dirty = true;

    public void Clear(AColor color)
    {
        _drawActions.Clear();
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
        if (!_dirty) return;

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
        if (targetWidth != Width || targetHeight != Height)
        {
            ResizeTexture(targetWidth, targetHeight);
            _width = targetWidth;
            _height = targetHeight;
        }

        BeginRender(_clearColor ?? AColor.FromRGBA(0, 0, 0, 0));
        foreach (var action in _drawActions)
            action.Execute(Target);
        EndRender();
        _dirty = false;
    }

    protected abstract void BeginRender(AColor clearColor);
    protected abstract void EndRender();
    protected abstract void ResizeTexture(int width, int height);
    protected abstract TTexture Target { get; }

    public abstract void SetPixel(APoint point, AColor color);
    public abstract void DrawLine(APoint start, APoint end, AColor color, float width = 1);
    public abstract void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1);
    public abstract void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1);
    public abstract void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1);
    public abstract void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1);
    public abstract void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format);
    public abstract void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position);
    public abstract void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular);
    public abstract void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular);
    public abstract IAbstTexture2D GetTexture(string? name = null);
    public abstract void Dispose();
}
