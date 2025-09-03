using AbstUI.SDL2.Components;
using AbstUI.SDL2.SDLL;
using System;

namespace AbstUI.SDL2.Core;

public class AbstSDLComponentContext : IDisposable
{
    private readonly AbstSDLComponentContainer _container;
    private bool _requireRender = true;
    internal IAbstSDLComponent? Component { get; private set; }
    internal AbstSDLComponentContext? LogicalParent { get; private set; }
    internal AbstSDLComponentContext? VisualParent { get; private set; }
    public event Action<IAbstSDLComponent> ? OnRequestRedraw;
    public nint Texture { get; private set; }
    public nint Renderer { get; set; }
    public int TargetWidth { get; set; }
    public int TargetHeight { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public float Blend { get; set; } = 100;
    public int? InkType { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public bool AlwaysOnTop { get; set; }
    public SDL.SDL_BlendMode BlendMode { get; set; } = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;

    internal AbstSDLComponentContext(
        AbstSDLComponentContainer container,
        IAbstSDLComponent? component = null,
        AbstSDLComponentContext? parent = null)
    {
        _container = container;
        LogicalParent = parent;
        VisualParent = parent;
        Component = component;
        _container.Register(this);
    }

    internal void SetParents(AbstSDLComponentContext? logicalParent, AbstSDLComponentContext? visualParent = null)
    {
        LogicalParent = logicalParent;
        VisualParent = visualParent ?? logicalParent;
    }

    public void QueueRedraw(IAbstSDLComponent component)
    {
        _requireRender = true;
        Component ??= component;
        VisualParent?.QueueRedraw(VisualParent.Component!);
        OnRequestRedraw?.Invoke(component);
    }

    public void RenderToTexture(AbstSDLRenderContext renderContext)
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
        SDL.SDL_Rect dst = new SDL.SDL_Rect
        {
            x = drawX,
            y = drawY,
            w = TargetWidth,
            h = TargetHeight
        };
        SDL.SDL_SetTextureAlphaMod(Texture, PercentToByte(Blend));
        SDL.SDL_SetTextureBlendMode(Texture, BlendMode);
        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;
        if (FlipH)
            flip |= SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
        if (FlipV)
            flip |= SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL;
        SDL.SDL_RenderCopyEx(Renderer, Texture, nint.Zero, ref dst, 0, nint.Zero, flip);
    }

    public void Dispose()
    {
        Texture = nint.Zero;
        _container.Unregister(this);
    }

    static byte PercentToByte(float percent) => (byte)Math.Clamp(percent * 2.55f, 0, 255);
}
