using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Components.Containers
{
    internal class ContainerHelpers
    {
        public static void HandleChildEvents(AbstSdlComponent comp, AbstSDLEvent e, int xOffset, int yOffset)
        {
            if (comp is not IHandleSdlEvent handler)
                return;
            ref var ev = ref e.Event;
            bool inside = true;
            int oldX = 0, oldY = 0;

            switch (ev.type)
            {
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    oldX = ev.button.x;
                    oldY = ev.button.y;
                    ev.button.x += xOffset;
                    ev.button.y += yOffset;
                    inside = ev.button.x >= comp.X && ev.button.x <= comp.X + comp.Width &&
                                ev.button.y >= comp.Y && ev.button.y <= comp.Y + comp.Height;
                    break;
                case SDL_EventType.SDL_MOUSEMOTION:
                    oldX = ev.motion.x;
                    oldY = ev.motion.y;
                    ev.motion.x += xOffset;
                    ev.motion.y += yOffset;
                    inside = ev.motion.x >= comp.X && ev.motion.x <= comp.X + comp.Width &&
                                ev.motion.y >= comp.Y && ev.motion.y <= comp.Y + comp.Height;
                    break;
                case SDL_EventType.SDL_MOUSEWHEEL:
                    SDL_GetMouseState(out var mx, out var my);
                    mx += xOffset;
                    my += yOffset;
                    inside = mx >= comp.X && mx <= comp.X + comp.Width &&
                                my >= comp.Y && my <= comp.Y + comp.Height;
                    break;
                default:
                    inside = true;
                    break;
            }

            if (inside)
                handler.HandleEvent(e);

            // Restore event coordinates for next child
            switch (ev.type)
            {
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    ev.button.x = oldX;
                    ev.button.y = oldY;
                    break;
                case SDL_EventType.SDL_MOUSEMOTION:
                    ev.motion.x = oldX;
                    ev.motion.y = oldY;
                    break;
            }
        }
    }
}
