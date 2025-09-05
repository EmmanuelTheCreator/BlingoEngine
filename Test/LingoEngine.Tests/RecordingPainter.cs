using System;
using System.Collections.Generic;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace LingoEngine.Tests;

internal sealed class RecordingPainter : IAbstImagePainter
{
    public int Height { get; set; }
    public int Width { get; set; }
    public bool Pixilated { get; set; }
    public bool AutoResize { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<DrawSingleLineCall> TextCalls { get; } = new();
    public List<LineCall> LineCalls { get; } = new();

    public void Clear(AColor color) { }
    public void SetPixel(APoint point, AColor color) { }
    public void DrawLine(APoint start, APoint end, AColor color, float width = 1) =>
        LineCalls.Add(new LineCall(start, end));
    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1) { }
    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1) { }
    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1) { }
    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1) { }
    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        int bottom = (int)MathF.Ceiling(position.Y + fontSize);
        int right = (int)MathF.Ceiling(position.X + width);
        if (right > Width) Width = right;
        if (bottom > Height) Height = bottom;
    }

    public void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular)
    {
        int bottom = (int)MathF.Ceiling(position.Y + height);
        int right = (int)MathF.Ceiling(position.X + width);
        if (right > Width) Width = right;
        if (bottom > Height) Height = bottom;
        TextCalls.Add(new DrawSingleLineCall(position, text, fontSize, width, height));
    }
    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format) { }
    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position) { }
    public IAbstTexture2D GetTexture(string? name = null) => null!;
    public void Render() { }
    public void Dispose() { }

    public readonly record struct DrawSingleLineCall(APoint Position, string Text, int FontSize, int Width, int Height);
    public readonly record struct LineCall(APoint Start, APoint End);
}
