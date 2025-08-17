using System;
using System.Numerics;

namespace AbstUI.ImGui;

public class AbstImGuiComponentContext : IDisposable
{
    private readonly AbstImGuiComponentContainer _container;
    private bool _requireRender = true;
    internal IAbstImGuiComponent? Component { get; private set; }
    internal AbstImGuiComponentContext? Parent { get; }

    public nint Texture { get; private set; }
    public nint Renderer { get; set; }
    public int TargetWidth { get; set; }
    public int TargetHeight { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; }
    public float Blend { get; set; }
    public int? InkType { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public object? BlendMode { get; set; }

    internal AbstImGuiComponentContext(AbstImGuiComponentContainer container, IAbstImGuiComponent? component = null, AbstImGuiComponentContext? parent = null)
    {
        _container = container;
        Parent = parent;
        Component = component;
        _container.Register(this);
    }

    public void QueueRedraw(IAbstImGuiComponent component)
    {
        _requireRender = true;
        Component ??= component;
        Parent?.QueueRedraw(Parent.Component!);
    }

    public void RenderToTexture(AbstImGuiRenderContext renderContext)
    {
        if (!Visible)
            return;

        Renderer = renderContext.Renderer;

        if (_requireRender && Component is { })
        {
            var renderResult = Component.Render(renderContext);
            Texture = renderResult.Texture;
            _requireRender = renderResult.DoRender;
        }

        if (Texture == nint.Zero)
            return;

        int drawX = (int)(X + OffsetX);
        int drawY = (int)(Y + OffsetY);

        var p0 = renderContext.Origin + new Vector2(drawX, drawY);
        var p1 = p0 + new Vector2(TargetWidth, TargetHeight);
        var uv0 = new Vector2(FlipH ? 1f : 0f, FlipV ? 1f : 0f);
        var uv1 = new Vector2(FlipH ? 0f : 1f, FlipV ? 0f : 1f);
        float alpha = Blend <= 0 ? 1f : Math.Clamp(Blend, 0f, 1f);
        var col = global::ImGuiNET.ImGui.GetColorU32(new Vector4(1f, 1f, 1f, alpha));
        renderContext.ImDrawList.AddImage(Texture, p0, p1, uv0, uv1, col);
    }

    public void Dispose()
    {
        Texture = nint.Zero;
        _container.Unregister(this);
    }
}
