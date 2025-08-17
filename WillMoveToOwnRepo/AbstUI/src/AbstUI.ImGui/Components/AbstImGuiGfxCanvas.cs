using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Components;
using AbstUI.ImGui.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.ImGui.Styles;
using ImGuiNET;

namespace AbstUI.ImGui.Components;

/// <summary>
/// Graphics canvas that renders primitives directly using ImGui draw lists.
/// </summary>
internal class AbstImGuiGfxCanvas : AbstImGuiComponent, IAbstFrameworkGfxCanvas, IDisposable
{
    public AMargin Margin { get; set; } = AMargin.Zero;

    private readonly List<Action<ImDrawListPtr, Vector2>> _drawActions = new();
    private AColor? _clearColor;

    public object FrameworkNode => this;

    public bool Pixilated { get; set; }
    private readonly ImGuiFontManager _fontManager;

    public AbstImGuiGfxCanvas(AbstImGuiComponentFactory factory, ImGuiFontManager fontManager, int width, int height)
        : base(factory)
    {
        _fontManager = fontManager;
        Width = width;
        Height = height;
    }

    public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
    {
        if (!Visibility)
            return nint.Zero;

        var screenPos = context.Origin + new Vector2(X, Y);
        ImGui.SetCursorScreenPos(screenPos);
        ImGui.PushID(Name);
        ImGui.InvisibleButton("canvas", new Vector2(Width, Height));
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetItemRectMin();

        if (_clearColor.HasValue)
            drawList.AddRectFilled(origin, origin + new Vector2(Width, Height),
                ImGui.GetColorU32(_clearColor.Value.ToImGuiColor()));

        foreach (var action in _drawActions)
            action(drawList, origin);

        ImGui.PopID();
        return AbstImGuiRenderResult.RequireRender();
    }

    private void MarkDirty() { }

    private static Vector2 ToVec2(APoint p) => new(p.X, p.Y);
    private static uint ToU32(AColor c) => ImGui.GetColorU32(c.ToImGuiColor());

    public void Clear(AColor color)
    {
        _drawActions.Clear();
        _clearColor = color;
        MarkDirty();
    }

    public void SetPixel(APoint point, AColor color)
    {
        _drawActions.Add((dl, origin) =>
            dl.AddRectFilled(origin + ToVec2(point), origin + ToVec2(point) + new Vector2(1, 1), ToU32(color)));
        MarkDirty();
    }

    public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        _drawActions.Add((dl, origin) =>
            dl.AddLine(origin + ToVec2(start), origin + ToVec2(end), ToU32(color), width));
        MarkDirty();
    }

    public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        _drawActions.Add((dl, origin) =>
        {
            var p0 = origin + new Vector2(rect.Left, rect.Top);
            var p1 = origin + new Vector2(rect.Right, rect.Bottom);
            if (filled)
                dl.AddRectFilled(p0, p1, ToU32(color));
            else
                dl.AddRect(p0, p1, ToU32(color), 0f, ImDrawFlags.None, width);
        });
        MarkDirty();
    }

    public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        _drawActions.Add((dl, origin) =>
        {
            var c = origin + ToVec2(center);
            if (filled)
                dl.AddCircleFilled(c, radius, ToU32(color));
            else
                dl.AddCircle(c, radius, ToU32(color), 0, width);
        });
        MarkDirty();
    }

    public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        _drawActions.Add((dl, origin) =>
        {
            var c = origin + ToVec2(center);
            float a0 = MathF.PI / 180f * startDeg;
            float a1 = MathF.PI / 180f * endDeg;
            dl.PathArcTo(c, radius, a0, a1, segments);
            dl.PathStroke(ToU32(color), ImDrawFlags.None, width);
        });
        MarkDirty();
    }

    public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        _drawActions.Add((dl, origin) =>
        {
            var arr = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
                arr[i] = origin + ToVec2(points[i]);
            if (filled)
                dl.AddConvexPolyFilled(arr, ToU32(color));
            else
                dl.AddPolyline(arr, ToU32(color), ImDrawFlags.None, width);
        });
        MarkDirty();
    }

    public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default)
    {
        var c = color ?? AColors.Black;
        _drawActions.Add((dl, origin) =>
            dl.AddText(origin + ToVec2(position), ToU32(c), text));
        MarkDirty();
    }

    public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        // Loading raw image data is not yet supported.
        MarkDirty();
    }

    public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        if (texture is ImGuiTexture2D img)
        {
            _drawActions.Add((dl, origin) =>
            {
                var p0 = origin + ToVec2(position);
                var p1 = p0 + new Vector2(width, height);
                dl.AddImage(img.Handle, p0, p1);
            });
        }
        MarkDirty();
    }

    public override void Dispose()
    {
        _drawActions.Clear();
        base.Dispose();
    }
}
