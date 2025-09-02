using AbstUI.Components;
using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Events;

namespace AbstUI.SDL2.Components.Containers
{
    internal class ContainerHelpers
    {
        public static void HandleChildEvents(List<IAbstFrameworkNode> _children, AbstSDLEvent e, float offsetX, float offsetY)
        {
            // Forward mouse events to children accounting for current scroll offset
            var oriOffsetX = e.OffsetX;
            var oriOffsetY = e.OffsetY;
            for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            {
                var comp = _children[i].FrameworkNode as AbstSdlComponent;
                HandleChild(e, offsetX, offsetY, oriOffsetX, oriOffsetY, comp);
            }
            e.OffsetX = oriOffsetX;
            e.OffsetY = oriOffsetY;
        }
        public static void HandleChildEvents(List<IAbstFrameworkLayoutNode> _children, AbstSDLEvent e, float offsetX, float offsetY)
        {
            // Forward mouse events to children accounting for current scroll offset
            var oriOffsetX = e.OffsetX;
            var oriOffsetY = e.OffsetY;
            for (int i = _children.Count - 1; i >= 0 && !e.StopPropagation; i--)
            {
                var comp = _children[i].FrameworkNode as AbstSdlComponent;
                HandleChild(e, offsetX, offsetY, oriOffsetX, oriOffsetY, comp);
            }
            e.OffsetX = oriOffsetX;
            e.OffsetY = oriOffsetY;
        }

        private static void HandleChild(AbstSDLEvent e, float offsetX, float offsetY, float oriOffsetX, float oriOffsetY, AbstSdlComponent? comp)
        {
            if (comp == null || comp is not IHandleSdlEvent handler || !comp.Visibility)
                return;
            
            if (comp is IAbstFrameworkLayoutNode layoutNode)
            {
                e.OffsetX = oriOffsetX - offsetX - layoutNode.X;
                e.OffsetY = oriOffsetY - offsetY - layoutNode.Y;
            }
            else
            {
                e.OffsetX = oriOffsetX - offsetX - comp.X;
                e.OffsetY = oriOffsetY - offsetY - comp.Y;
            }
                HandleChildEvents(comp, e);
        }

        public static void HandleChildEvents(AbstSdlComponent comp, AbstSDLEvent e)
        {
            if (comp is not IHandleSdlEvent handler)
                return;
            var ev = e.Event;
            if (ev.type == SDLL.SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {

            }
            
            e.CalulateIsInside(comp.Width, comp.Height);
            if (comp.Name.Contains("btn1"))
            {
                Console.WriteLine($"Even0 {ev.type} at {e.ComponentLeft}x{e.ComponentTop}\t({e.OffsetX}x{e.OffsetY}) inside={e.IsInside} \t {comp.Name}");
            }
            if (handler.CanHandleEvent(e))
            {
                Console.WriteLine($"Even1 {ev.type} at {e.ComponentLeft}x{e.ComponentTop}\t({e.OffsetX}x{e.OffsetY}) inside={e.IsInside} \t {comp.Name}");
                if (comp.Name.Contains("wrap2"))
                {

                }
                handler.HandleEvent(e);
            }
        }
    }
}
