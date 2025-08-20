namespace AbstUI.SDL2.Components;

public interface ISdlFocusable
{
    bool HasFocus { get; }
    void SetFocus(bool focus);
}
