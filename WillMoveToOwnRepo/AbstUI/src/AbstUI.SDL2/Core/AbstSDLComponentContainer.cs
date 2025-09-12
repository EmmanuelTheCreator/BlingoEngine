using AbstUI.SDL2.Events;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Core;

public class AbstSDLComponentContainer
{
    private readonly List<AbstSDLComponentContext> _allComponents = new();
    private readonly List<AbstSDLComponentContext> _activeComponents = new();
    private readonly SdlFocusManager _focusManager;

    public AbstSDLComponentContainer(SdlFocusManager focusManager)
    {
        _focusManager = focusManager;
    }

    internal void Register(AbstSDLComponentContext context) => _allComponents.Add(context);
    internal void Unregister(AbstSDLComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(AbstSDLComponentContext context)
    {
        _activeComponents.Remove(context);
        int idx = _activeComponents.FindIndex(c =>
            (c.AlwaysOnTop && !context.AlwaysOnTop) ||
            (c.AlwaysOnTop == context.AlwaysOnTop && c.ZIndex > context.ZIndex));
        if (idx >= 0)
            _activeComponents.Insert(idx, context);
        else
            _activeComponents.Add(context);
    }

    public void Deactivate(AbstSDLComponentContext context) => _activeComponents.Remove(context);

    /// <summary>
    /// Requests a redraw for every registered component.
    /// Useful when a global event like a window resize requires
    /// all cached textures to be regenerated.
    /// </summary>
    public void QueueRedrawAll()
    {
        foreach (var ctx in _allComponents)
        {
            if (ctx.Component != null)
                ctx.QueueRedraw(ctx.Component);
        }
    }

    public void Render(AbstSDLRenderContext renderContext)
    {
        foreach (var ctx in _activeComponents)
        {
            ctx.RenderToTexture(renderContext);
        }
    }

    public void HandleEvent(SDL.SDL_Event e)
    {
        if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
            _focusManager.SetFocus(null);
        var handled = false;
        var evt = new AbstSDLEvent(e);
        for (int i = _activeComponents.Count - 1; i >= 0; i--)
        {
            var ctx = _activeComponents[i];
            if (ctx.Component is not IHandleSdlEvent handler)
                continue;
            var okEvent = false;
            bool inside = true;
            switch (evt.Event.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    var bx = evt.Event.button.x;
                    var by = evt.Event.button.y;
                    inside = bx >= ctx.X && bx <= ctx.X + ctx.TargetWidth &&
                             by >= ctx.Y && by <= ctx.Y + ctx.TargetHeight;
                    okEvent = true;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    var mx = evt.Event.motion.x;
                    var my = evt.Event.motion.y;
                    inside = mx >= ctx.X && mx <= ctx.X + ctx.TargetWidth &&
                             my >= ctx.Y && my <= ctx.Y + ctx.TargetHeight;
                    okEvent = true;
                    //Console.WriteLine($"Source: {mx}x{my}\t{ctx.X}x{ctx.Y} {inside}");
                    break;
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    SDL.SDL_GetMouseState(out var wx, out var wy);
                    inside = wx >= ctx.X && wx <= ctx.X + ctx.TargetWidth &&
                             wy >= ctx.Y && wy <= ctx.Y + ctx.TargetHeight;
                    okEvent = true;
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                case SDL.SDL_EventType.SDL_TEXTEDITING:
                case SDL.SDL_EventType.SDL_TEXTEDITING_EXT:
                case SDL.SDL_EventType.SDL_CLIPBOARDUPDATE:
                    okEvent = true;
                    break;
            }
            if (inside && okEvent)
            {
                handled = true;
                handler.HandleEvent(evt);
                if (evt.StopPropagation)
                    break;
            }
        }
        if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && !handled)
            _focusManager.SetFocus(null);
    }
}
