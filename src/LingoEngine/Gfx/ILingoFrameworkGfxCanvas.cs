using LingoEngine.Bitmaps;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific drawing surface implementation.
    /// </summary>
    public interface ILingoFrameworkGfxCanvas : ILingoFrameworkGfxLayoutNode
    {
        bool Pixilated { get; set; }
        void Clear(LingoColor color);
        void SetPixel(LingoPoint point, LingoColor color);
        void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1);
        void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1);
        void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1);
        void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1);
        void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1);
        void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12, int width = -1, Texts.LingoTextAlignment alignment = default);
        void DrawPicture(byte[] data, int width, int height, LingoPoint position, LingoPixelFormat format);
        void DrawPicture(ILingoTexture2D texture, int width, int height, LingoPoint position);
    }
}
