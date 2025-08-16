using System;

namespace AbstUI.Blazor;

public class AbstBlazorComponentContext : IDisposable
{
    private readonly AbstBlazorComponentContainer _container;
    private bool _requireRender = true;
    internal IAbstBlazorComponent? Component { get; private set; }
    internal AbstBlazorComponentContext? Parent { get; }

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
    public Blazor.Blazor_BlendMode BlendMode { get; set; } = Blazor.Blazor_BlendMode.Blazor_BLENDMODE_BLEND;

    internal AbstBlazorComponentContext(AbstBlazorComponentContainer container, IAbstBlazorComponent? component = null, AbstBlazorComponentContext? parent = null)
    {
        _container = container;
        Parent = parent;
        Component = component;
        _container.Register(this);
    }

    public void QueueRedraw(IAbstBlazorComponent component)
    {
        _requireRender = true;
        Component ??= component;
        Parent?.QueueRedraw(Parent.Component!);
    }

    public void RenderToTexture(AbstBlazorRenderContext renderContext)
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
        Blazor.Blazor_Rect dst = new Blazor.Blazor_Rect
        {
            x = drawX,
            y = drawY,
            w = TargetWidth,
            h = TargetHeight
        };
        Blazor.Blazor_SetTextureAlphaMod(Texture, (byte)(Blend * 255));
        Blazor.Blazor_SetTextureBlendMode(Texture, BlendMode);
        Blazor.Blazor_RendererFlip flip = Blazor.Blazor_RendererFlip.Blazor_FLIP_NONE;
        if (FlipH)
            flip |= Blazor.Blazor_RendererFlip.Blazor_FLIP_HORIZONTAL;
        if (FlipV)
            flip |= Blazor.Blazor_RendererFlip.Blazor_FLIP_VERTICAL;
        Blazor.Blazor_RenderCopyEx(Renderer, Texture, nint.Zero, ref dst, 0, nint.Zero, flip);
    }

    public void Dispose()
    {
        Texture = nint.Zero;
        _container.Unregister(this);
    }
}
