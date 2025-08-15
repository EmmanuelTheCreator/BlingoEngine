using LingoEngine.Director.Core.Sprites;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Sprites;
using LingoEngine.Inputs;
using LingoEngine.Events;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Members;
using LingoEngine.Texts;
using LingoEngine.AbstUI.Primitives;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Overlay that draws bounding boxes with resize anchors for selected sprites.
/// </summary>
public class StageBoundingBoxesOverlay : IHasSpriteSelectedEvent, ILingoMouseEventHandler, IDisposable
{
    private readonly LingoGfxCanvas _canvas;
    private readonly IDirectorEventMediator _mediator;
    private ILingoStageMouse _mouse;
    private ILingoKey _key;
    private readonly List<LingoSprite2D> _sprites = new();
    private LingoSprite2D? _primary;

    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public LingoGfxCanvas Canvas => _canvas;

    public StageBoundingBoxesOverlay(ILingoFrameworkFactory factory, IDirectorEventMediator mediator)
    {
        _mediator = mediator;
        _mouse = null!;
        _key = null!;
        _canvas = factory.CreateGfxCanvas("BoundingBoxesCanvas", 640, 480);
        _mediator.Subscribe(this);
        Visible = false;
    }

    public void SetInput(ILingoStageMouse mouse, ILingoKey key)
    {
        if (_mouse != null)
            _mouse.Unsubscribe(this);
        _mouse = mouse;
        _key = key;
        _mouse.Subscribe(this);
    }

    public void Dispose()
    {
        _mediator.Unsubscribe(this);
        if (_mouse != null)
            _mouse.Unsubscribe(this);
        _canvas.Dispose();
    }

    public void SetSprites(IEnumerable<LingoSprite2D> sprites)
    {
        _sprites.Clear();
        _sprites.AddRange(sprites);
        Draw();
    }

    public void SpriteSelected(ILingoSpriteBase sprite)
    {
        _primary = sprite as LingoSprite2D;
        if (_primary != null && !_sprites.Contains(_primary))
        {
            _sprites.Clear();
            _sprites.Add(_primary);
        }
        Draw();
    }

    private enum Anchor
    {
        None,
        TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left
    }

    private Anchor _dragAnchor = Anchor.None;
    private float _startLocH, _startLocV, _startWidth, _startHeight;
    private float _startMouseX, _startMouseY;

    private void OnMouseDown(LingoMouseEvent mouse)
    {
        if (_primary == null) return;
        var anchor = HitTest(mouse.MouseH, mouse.MouseV);
        if (anchor != Anchor.None)
        {
            _dragAnchor = anchor;
            _startLocH = _primary.LocH;
            _startLocV = _primary.LocV;
            _startWidth = _primary.Width;
            _startHeight = _primary.Height;
            _startMouseX = mouse.MouseH;
            _startMouseY = mouse.MouseV;
        }
    }

    void ILingoMouseEventHandler.RaiseMouseDown(LingoMouseEvent mouse) => OnMouseDown(mouse);

    private void OnMouseUp(LingoMouseEvent mouse)
    {
        _dragAnchor = Anchor.None;
    }

    void ILingoMouseEventHandler.RaiseMouseUp(LingoMouseEvent mouse) => OnMouseUp(mouse);

    private void OnMouseMove(LingoMouseEvent mouse)
    {
        if (_dragAnchor == Anchor.None || _primary == null) return;
        float dx = mouse.MouseH - _startMouseX;
        float dy = mouse.MouseV - _startMouseY;
        bool alt = _key.OptionDown;

        float locH = _startLocH;
        float locV = _startLocV;
        float width = _startWidth;
        float height = _startHeight;

        switch (_dragAnchor)
        {
            case Anchor.TopLeft:
                if (alt) { width = _startWidth - 2 * dx; height = _startHeight - 2 * dy; }
                else { width = _startWidth - dx; height = _startHeight - dy; locH = _startLocH + dx / 2; locV = _startLocV + dy / 2; }
                break;
            case Anchor.Top:
                if (alt) { height = _startHeight - 2 * dy; }
                else { height = _startHeight - dy; locV = _startLocV + dy / 2; }
                break;
            case Anchor.TopRight:
                if (alt) { width = _startWidth + 2 * dx; height = _startHeight - 2 * dy; }
                else { width = _startWidth + dx; height = _startHeight - dy; locH = _startLocH + dx / 2; locV = _startLocV + dy / 2; }
                break;
            case Anchor.Right:
                if (alt) { width = _startWidth + 2 * dx; }
                else { width = _startWidth + dx; locH = _startLocH + dx / 2; }
                break;
            case Anchor.BottomRight:
                if (alt) { width = _startWidth + 2 * dx; height = _startHeight + 2 * dy; }
                else { width = _startWidth + dx; height = _startHeight + dy; locH = _startLocH + dx / 2; locV = _startLocV + dy / 2; }
                break;
            case Anchor.Bottom:
                if (alt) { height = _startHeight + 2 * dy; }
                else { height = _startHeight + dy; locV = _startLocV + dy / 2; }
                break;
            case Anchor.BottomLeft:
                if (alt) { width = _startWidth - 2 * dx; height = _startHeight + 2 * dy; }
                else { width = _startWidth - dx; height = _startHeight + dy; locH = _startLocH + dx / 2; locV = _startLocV + dy / 2; }
                break;
            case Anchor.Left:
                if (alt) { width = _startWidth - 2 * dx; }
                else { width = _startWidth - dx; locH = _startLocH + dx / 2; }
                break;
        }

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        _primary.LocH = locH;
        _primary.LocV = locV;
        _primary.Width = width;
        _primary.Height = height;
        Draw();
    }

    void ILingoMouseEventHandler.RaiseMouseMove(LingoMouseEvent mouse) => OnMouseMove(mouse);
    void ILingoMouseEventHandler.RaiseMouseWheel(LingoMouseEvent mouse) { }

    private Anchor HitTest(float x, float y)
    {
        if (_primary == null) return Anchor.None;
        var r = _primary.Rect;
        var points = GetAnchorPoints(r).ToArray();
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (x >= p.X - 1 && x <= p.X + 1 && y >= p.Y - 1 && y <= p.Y + 1)
                return (Anchor)i + 1; // Anchor enum after None
        }
        return Anchor.None;
    }

    private IEnumerable<APoint> GetAnchorPoints(ARect r)
    {
        yield return new APoint(r.Left, r.Top); // TL
        yield return new APoint((r.Left + r.Right) / 2, r.Top); // T
        yield return new APoint(r.Right, r.Top); // TR
        yield return new APoint(r.Right, (r.Top + r.Bottom) / 2); // R
        yield return new APoint(r.Right, r.Bottom); // BR
        yield return new APoint((r.Left + r.Right) / 2, r.Bottom); // B
        yield return new APoint(r.Left, r.Bottom); // BL
        yield return new APoint(r.Left, (r.Top + r.Bottom) / 2); // L
    }

    private void Draw()
    {
        _canvas.Clear(AColors.Transparent);
        foreach (var sprite in _sprites)
        {
            if (sprite.Member is LingoMemberText || sprite.Member is LingoMemberField)
            {
                var r = sprite.Rect;
                _canvas.DrawRect(ARect.New(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top), AColors.Yellow, false, 1);
                foreach (var p in GetAnchorPoints(r))
                    _canvas.DrawRect(ARect.New(p.X - 1, p.Y - 1, 2, 2), AColors.Yellow, true);
            }
        }
    }
}
