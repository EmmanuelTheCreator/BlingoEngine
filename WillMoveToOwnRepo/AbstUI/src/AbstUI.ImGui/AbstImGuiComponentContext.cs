using System;
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
        // TODO: draw the texture directly with ImGui here
    }

    public void Dispose()
    {
        Texture = nint.Zero;
        _container.Unregister(this);
    }
}
