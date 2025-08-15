using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Components
{
    /// <summary>
    /// High level drawing surface used by the engine.
    /// Rendering back-ends provide the <see cref="IAbstUIFrameworkGfxCanvas"/>
    /// implementation which performs the actual drawing operations.
    /// </summary>
    public class AbstUIGfxCanvas : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxCanvas>
    {
        public bool Pixilated { get => _framework.Pixilated; set => _framework.Pixilated = value; }
        public void Clear(AColor color) => _framework.Clear(color);
        public void SetPixel(APoint point, AColor color) => _framework.SetPixel(point, color);
        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
            => _framework.DrawLine(start, end, color, width);
        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
            => _framework.DrawRect(rect, color, filled, width);
        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
            => _framework.DrawCircle(center, radius, color, filled, width);
        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
            => _framework.DrawArc(center, radius, startDeg, endDeg, segments, color, width);
        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
            => _framework.DrawPolygon(points, color, filled, width);
        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left)
            => _framework.DrawText(position, text, font, color, fontSize, width, alignment);
        public void DrawPicture(IAbstUITexture2D texture, int width, int height, APoint position)
            => _framework.DrawPicture(texture, width, height, position);
        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
            => _framework.DrawPicture(data, width, height, position, format);
    }

}
