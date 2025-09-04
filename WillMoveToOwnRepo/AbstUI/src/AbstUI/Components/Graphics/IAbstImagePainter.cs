using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;

namespace AbstUI.Components.Graphics
{
    public interface IAbstImagePainter : IDisposable
    {
        int Height { get; set; }
        int Width { get; set; }
        bool Pixilated { get; set; }
        bool AutoResize { get; set; }

        void Clear(AColor color);
        void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1);
        void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1);
        void DrawLine(APoint start, APoint end, AColor color, float width = 1);
        void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format);
        void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position);
        void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1);
        void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1);
        void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular);
        void DrawSingleLine(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = AbstTextAlignment.Left, AbstFontStyle style = AbstFontStyle.Regular);
        void SetPixel(APoint point, AColor color);
        IAbstTexture2D GetTexture(string? name = null);
        void Render();
    }
}