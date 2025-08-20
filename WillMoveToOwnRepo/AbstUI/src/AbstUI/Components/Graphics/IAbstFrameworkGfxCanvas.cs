using AbstUI.Primitives;
using AbstUI.Texts;

namespace AbstUI.Components.Graphics
{
    /// <summary>
    /// Framework specific drawing surface implementation.
    /// </summary>
    public interface IAbstFrameworkGfxCanvas : IAbstFrameworkLayoutNode
    {
        bool Pixilated { get; set; }
        void Clear(AColor color);
        void SetPixel(APoint point, AColor color);
        void DrawLine(APoint start, APoint end, AColor color, float width = 1);
        void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1);
        void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1);
        void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1);
        void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1);
        void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default);
        void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format);
        void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position);
    }
}
