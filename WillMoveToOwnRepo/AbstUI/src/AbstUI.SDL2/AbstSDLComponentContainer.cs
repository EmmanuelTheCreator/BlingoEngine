using System.Collections.Generic;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2;

public class AbstSDLComponentContainer
{
    private readonly List<AbstSDLComponentContext> _allComponents = new();
    private readonly HashSet<AbstSDLComponentContext> _activeComponents = new();

    internal void Register(AbstSDLComponentContext context) => _allComponents.Add(context);
    internal void Unregister(AbstSDLComponentContext context)
    {
        _activeComponents.Remove(context);
        _allComponents.Remove(context);
    }

    public void Activate(AbstSDLComponentContext context) => _activeComponents.Add(context);
    public void Deactivate(AbstSDLComponentContext context) => _activeComponents.Remove(context);

    public void Render(AbstSDLRenderContext renderContext)
    {
        foreach (var ctx in _activeComponents)
        {
            ctx.RenderToTexture(renderContext);
        }
    }

    public void HandleEvent(SDL.SDL_Event e)
    {
        var evt = new AbstSDLEvent(e);
        foreach (var ctx in _activeComponents)
        {
            if (ctx.Component is not IHandleSdlEvent handler)
                continue;

            bool inside = true;
            switch (evt.Event.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    var bx = evt.Event.button.x;
                    var by = evt.Event.button.y;
                    inside = bx >= ctx.X && bx <= ctx.X + ctx.TargetWidth &&
                             by >= ctx.Y && by <= ctx.Y + ctx.TargetHeight;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    var mx = evt.Event.motion.x;
                    var my = evt.Event.motion.y;
                    inside = mx >= ctx.X && mx <= ctx.X + ctx.TargetWidth &&
                             my >= ctx.Y && my <= ctx.Y + ctx.TargetHeight;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    SDL.SDL_GetMouseState(out var wx, out var wy);
                    inside = wx >= ctx.X && wx <= ctx.X + ctx.TargetWidth &&
                             wy >= ctx.Y && wy <= ctx.Y + ctx.TargetHeight;
                    break;
            }

            if (inside)
            {
                handler.HandleEvent(evt);
                if (evt.StopPropagation)
                    break;
            }
        }
    }
}
