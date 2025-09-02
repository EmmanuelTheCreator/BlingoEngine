using AbstUI.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;

namespace AbstUI.SDL2.Components.Containers
{
    internal class ContainerHelpers
    {
        public static void HandleChildEvents(List<IAbstFrameworkLayoutNode> _children, AbstSDLEvent e, float offsetX, float offsetY)
        {
            // Forward mouse events to children accounting for current scroll offset
            var oriOffsetX = e.OffsetX;
            var oriOffsetY = e.OffsetY;
            for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            {
                if (_children[i].FrameworkNode is AbstSdlComponent comp && comp is IHandleSdlEvent handler && comp.Visibility)
                {
                    if (comp is IAbstFrameworkLayoutNode layoutNode)
                    {
                        e.OffsetX = oriOffsetX - offsetX - layoutNode.X;
                        e.OffsetY = oriOffsetY - offsetY - layoutNode.Y;
                    }
                    HandleChildEvents(comp, e);
                }
            }
            e.OffsetX = oriOffsetX;
            e.OffsetY = oriOffsetY;
        }
        public static void HandleChildEvents(AbstSdlComponent comp, AbstSDLEvent e)
        {
            if (comp is not IHandleSdlEvent handler)
                return;
            ref var ev = ref e.Event;
            
            var oldX = e.MouseX;
            var oldY = e.MouseY;
            var newCoorX = e.ComponentLeft;
            var newCoorY = e.ComponentTop;
            bool inside = false;
            var doCheck = true;
            if (ev.type == SDLL.SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {

            }
            //switch (ev.type)
            //{
            //    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
            //    case SDL_EventType.SDL_MOUSEBUTTONUP:
            //        ev.button.x += e.OffsetX;
            //        ev.button.y += e.OffsetY;
            //        newCoorX = ev.button.x;
            //        newCoorY = ev.button.y;
            //        doCheck = true;
            //        break;
            //    case SDL_EventType.SDL_MOUSEMOTION:
            //        ev.motion.x += e.OffsetX;
            //        ev.motion.y += e.OffsetY;
            //        newCoorX = ev.motion.x;
            //        newCoorY = ev.motion.y;
            //        doCheck = true;
            //        break;
            //    case SDL_EventType.SDL_MOUSEWHEEL:
            //        newCoorX = e.OffsetX + e.MouseX;
            //        newCoorY = e.OffsetY + e.MouseY;
            //        doCheck = true;
            //        break;
            //    default:
            //        inside = true;
            //        break;
            //}
            //if (doCheck)
            e.CalulateIsInside(comp.Width, comp.Height);
            if (handler.CanHandleEvent(e))
            {
                Console.WriteLine($"Event {ev.type} at {e.ComponentLeft}x{e.ComponentTop}\t({e.OffsetX}x{e.OffsetY}) inside={inside} \t {comp.Name}");
                if (comp.Name.Contains("but"))
                {

                }
                handler.HandleEvent(e);
            }

            // Restore event coordinates for next child
            //switch (ev.type)
            //{
            //    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
            //    case SDL_EventType.SDL_MOUSEBUTTONUP:
            //        ev.button.x = (int)oldX;
            //        ev.button.y = (int)oldY;
            //        break;
            //    case SDL_EventType.SDL_MOUSEMOTION:
            //        ev.motion.x = (int)oldX;
            //        ev.motion.y = (int)oldY;
            //        break;
            //}
        }
    }
}
