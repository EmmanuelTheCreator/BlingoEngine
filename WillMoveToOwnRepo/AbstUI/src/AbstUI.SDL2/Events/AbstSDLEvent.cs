using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Events;

public class AbstSDLEvent
{
    public SDL.SDL_Event Event;
    public bool StopPropagation;
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public AbstSDLEvent(SDL.SDL_Event e)
    {
        Event = e;
    }
}
