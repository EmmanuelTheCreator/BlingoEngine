using System.ComponentModel;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Events;

public class AbstSDLEvent
{
    public SDL_Event Event;
    public bool StopPropagation;
    private float _offsetY;
    private float _offsetX;

    public float OffsetX
    {
        get => _offsetX;
        set
        {
            _offsetX = value;
            ComponentLeft = MouseX + _offsetX;
        }
    }
    public float OffsetY
    {
        get => _offsetY;
        set
        {
            _offsetY = value;
            ComponentTop = MouseY + _offsetY;
        }
    }
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    public float ComponentLeft { get; private set; }
    public float ComponentTop { get; private set; }
    public bool IsInside{ get; private set; }
    public bool CalulateIsInside(float width, float height)
    {
        IsInside =
                ComponentLeft >= 0 && ComponentLeft <= width &&
                ComponentTop  >= 0 && ComponentTop <= height;
        return IsInside;
    }
    /// <summary>
    /// If the event has coordinates (like mouse or touch events), it is false for keyboard events.
    /// </summary>
    public bool HasCoordinates { get; private set; }
    public AbstSDLEvent(SDL_Event e)
    {
        Event = e;
        switch (e.type)
        {
            case SDL_EventType.SDL_MOUSEMOTION:
                MouseX = e.motion.x;
                MouseY = e.motion.y;
                HasCoordinates = true;
                break;
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
            case SDL_EventType.SDL_MOUSEBUTTONUP:
                MouseX = e.button.x;
                MouseY = e.button.y;
                HasCoordinates = true;
                break;
            case SDL_EventType.SDL_FINGERDOWN:
            case SDL_EventType.SDL_FINGERUP:
            case SDL_EventType.SDL_FINGERMOTION:
                MouseX = e.tfinger.x;
                MouseY = e.tfinger.y;
                HasCoordinates = true;
                break;
            case SDL_EventType.SDL_MOUSEWHEEL:
                SDL_GetMouseState(out var mx, out var my);
                MouseX = mx;
                MouseY = my;
                HasCoordinates = true;
                break;
            default:
                HasCoordinates = false;
                break;
        }
    }
}
