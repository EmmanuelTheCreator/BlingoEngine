namespace AbstUI.SDL2.Events;

internal interface IHandleSdlEvent
{
    bool CanHandleEvent(AbstSDLEvent e) => true;
    void HandleEvent(AbstSDLEvent e);
}
