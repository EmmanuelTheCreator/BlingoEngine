using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Bitmaps;
using AbstUI.Components.Graphics;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Texts;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Godot;

namespace AbstUI.LGodot.Components.Graphics;

public partial class GodotImagePainter : IAbstImagePainter
{
    private readonly AbstGodotFontManager _fontManager;
    private readonly List<(Func<APoint?> GetTotalSize, Action DrawAction)> _drawActions = new();
    private readonly DrawingControl _control;
    private AColor? _clearColor;
    private bool _dirty;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool AutoResize { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool Pixilated
    {
        get => _control.TextureFilter == CanvasItem.TextureFilterEnum.Nearest;
        set => _control.TextureFilter = value ? CanvasItem.TextureFilterEnum.Nearest : CanvasItem.TextureFilterEnum.Linear;
    }

    public Control GodotControl => _control;

    int IAbstImagePainter.Height { get => Height; set => Resize(Width, value); }
    int IAbstImagePainter.Width { get => Width; set => Resize(value, Height); }

    public GodotImagePainter(AbstGodotFontManager fontManager, int width = 0, int height = 0)
    {
        _fontManager = fontManager;
        if (width == 0) width = 10;
        if (height == 0) height = 10;
        Width = width;
        Height = height;
        _control = new DrawingControl(this)
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new Vector2(width, height),
            CustomMinimumSize = new Vector2(width, height)
        };
    }

    public void Resize(int width, int height)
    {
        if (Width == width && Height == height)
            return;
        Width = width;
        Height = height;
        _control.Size = new Vector2(width, height);
        _control.CustomMinimumSize = _control.Size;
        MarkDirty();
    }

    public void Dispose()
    {
        _drawActions.Clear();
        _control.QueueFree();
    }

    private void MarkDirty()
    {
        _dirty = true;
        _control.QueueRedraw();
    }

    public void Render()
    {
        if (!_dirty) return;
        if (AutoResize)
        {
            int newWidth = Width;
            int newHeight = Height;
            foreach (var (getSize, _) in _drawActions)
            {
                var size = getSize?.Invoke();
                if (size != null)
                {
                    if (size.Value.X > newWidth) newWidth = (int)size.Value.X;
                    if (size.Value.Y > newHeight) newHeight = (int)size.Value.Y;
                }
            }
            if (newWidth > Width || newHeight > Height)
                Resize(Math.Max(Width, newWidth), Math.Max(Height, newHeight));
        }
        _control.QueueRedraw();
    }

    public IAbstTexture2D GetTexture(string? name = null)
    {
        Render();
        return _control.CreateAbstTexture(name);
    }

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    public void SetPixel(APoint point, AColor color)
    {
        var p = point;
        var c = color.ToGodotColor();
        _drawActions.Add((
            () => AutoResize ? new APoint(p.X + 1, p.Y + 1) : null,
            () => _control.DrawRect(new Rect2(p.X, p.Y, 1, 1), c, true)));
        MarkDirty();
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start; var e = end; var c = color.ToGodotColor();
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
                int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
                return new APoint(maxX, maxY);
            },
            () => _control.DrawLine(s.ToVector2(), e.ToVector2(), c, width)));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect; var c = color.ToGodotColor();
        _drawActions.Add((
            () => AutoResize ? new APoint(r.Left + r.Width, r.Top + r.Height) : null,
            () =>
            {
                var godotRect = r.ToRect2();
                if (filled)
                    _control.DrawRect(godotRect, c, true);
                else
                    _control.DrawRect(godotRect, c, false, width);
            }
        ));
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var ctr = center; var c = color.ToGodotColor();
        _drawActions.Add((
            () => AutoResize ? new APoint((int)(ctr.X + radius + 1), (int)(ctr.Y + radius + 1)) : null,
            () =>
            {
                if (filled)
                    _control.DrawCircle(ctr.ToVector2(), radius, c);
                else
                    _control.DrawArc(ctr.ToVector2(), radius, 0, 360, 32, c, width);
            }
        ));
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var ctr = center; var c = color.ToGodotColor();
        _drawActions.Add((
            () => AutoResize ? new APoint((int)(ctr.X + radius + 1), (int)(ctr.Y + radius + 1)) : null,
            () => _control.DrawArc(ctr.ToVector2(), radius, startDeg, endDeg, segments, c, width)));
        MarkDirty();
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        var pts = points.Select(p => p).ToArray();
        var c = color.ToGodotColor();
        _drawActions.Add((
            () =>
            {
                if (!AutoResize) return null;
                int maxX = 0, maxY = 0;
                foreach (var p in pts)
                {
                    if (p.X > maxX) maxX = (int)p.X;
                    if (p.Y > maxY) maxY = (int)p.Y;
                }
                return new APoint(maxX + 1, maxY + 1);
            },
            () =>
            {
                var arr = pts.Select(p => p.ToVector2()).ToArray();
                if (filled)
                    _control.DrawPolygon(arr, new[] { c });
                else
                    _control.DrawPolyline(arr, c, width, true);
            }
        ));
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
        int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var col = (color ?? new AColor(0, 0, 0, 255)).ToGodotColor();
        var fontGodot = _fontManager.Get<FontFile>(font ?? string.Empty, style) ?? ThemeDB.FallbackFont;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize || string.IsNullOrEmpty(txt)) return null;
                if (!txt.Contains('\n'))
                {
                    float tw = width >= 0 ? width : fontGodot.GetStringSize(txt, alignment.ToGodot(), width, fontSize).X;
                    float th = fontGodot.GetHeight(fontSize);
                    return new APoint((int)(pos.X + tw), (int)(pos.Y + th));
                }
                else
                {
                    var lines = txt.Split('\n');
                    float lineHeight = fontGodot.GetHeight(fontSize);
                    float maxW = 0;
                    foreach (var line in lines)
                        maxW = MathF.Max(maxW, fontGodot.GetStringSize(line, alignment.ToGodot(), width, fontSize).X);
                    float h = lineHeight * lines.Length;
                    float w = width >= 0 ? width : maxW;
                    return new APoint((int)(pos.X + w), (int)(pos.Y + h));
                }
            },
            () =>
            {
                if (!txt.Contains('\n'))
                {
                    int w = width >= 0 ? width : -1;
                    _control.DrawString(fontGodot, pos.ToVector2(), txt, alignment.ToGodot(), w, fontSize, col);
                }
                else
                {
                    var lines = txt.Split('\n');
                    var lineHeight = fontGodot.GetHeight(fontSize);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var p = new Vector2(pos.X, pos.Y + i * lineHeight);
                        int w = width >= 0 ? width : -1;
                        _control.DrawString(fontGodot, p, lines[i], alignment.ToGodot(), w, fontSize, col);
                    }
                }
            }
        ));
        MarkDirty();
    }

    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12,
        int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left,
        AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var col = (color ?? new AColor(0, 0, 0, 255)).ToGodotColor();
        var fontGodot = _fontManager.Get<FontFile>(font ?? string.Empty, style) ?? ThemeDB.FallbackFont;
        _drawActions.Add((
            () =>
            {
                if (!AutoResize || string.IsNullOrEmpty(txt)) return null;
                float tw = width >= 0 ? width : fontGodot.GetStringSize(txt, alignment.ToGodot(), width, fontSize).X;
                float th = height >= 0 ? height : fontGodot.GetHeight(fontSize);
                return new APoint((int)(pos.X + tw), (int)(pos.Y + th));
            },
            () =>
            {
                int w = width >= 0 ? width : -1;
                _control.DrawString(fontGodot, pos.ToVector2(), txt, alignment.ToGodot(), w, fontSize, col);
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        using var img = Image.CreateFromData(width, height, false, format.ToGodotFormat(), data);
        var tex = ImageTexture.CreateFromImage(img);
        if (tex == null) return;
        var pos = position;
        _drawActions.Add((
            () => AutoResize ? new APoint((int)(pos.X + width), (int)(pos.Y + height)) : null,
            () =>
            {
                _control.DrawTexture(tex, pos.ToVector2());
                tex.Dispose();
            }
        ));
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        var tex = ((AbstGodotTexture2D)texture).Texture;
        var pos = position;
        _drawActions.Add((
            () => AutoResize ? new APoint((int)(pos.X + width), (int)(pos.Y + height)) : null,
            () => _control.DrawTextureRect(tex, new Rect2(pos.X, pos.Y, width, height), false)));
        MarkDirty();
    }

    private sealed partial class DrawingControl : Control
    {
        private readonly GodotImagePainter _parent;
        public DrawingControl(GodotImagePainter parent)
        {
            _parent = parent;
        }

        public override void _Draw()
        {
            if (_parent._clearColor.HasValue)
            {
                var c = _parent._clearColor.Value.ToGodotColor();
                DrawRect(new Rect2(0, 0, _parent.Width, _parent.Height), c, true);
            }
            foreach (var action in _parent._drawActions)
                action.DrawAction();
            _parent._dirty = false;
        }
    }
}
