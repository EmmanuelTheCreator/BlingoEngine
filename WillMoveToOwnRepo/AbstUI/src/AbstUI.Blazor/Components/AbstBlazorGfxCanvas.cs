using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorGfxCanvas : AbstBlazorComponentBase, IAbstFrameworkGfxCanvas
{
    [Parameter] public bool Pixilated { get; set; }

    public void Clear(AColor color) { }
    public void SetPixel(APoint point, AColor color) { }
    public void DrawLine(APoint start, APoint end, AColor color, float width = 1) { }
    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1) { }
    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1) { }
    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1) { }
    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1) { }
    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, Texts.AbstTextAlignment alignment = default) { }
    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format) { }
    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position) { }
}
