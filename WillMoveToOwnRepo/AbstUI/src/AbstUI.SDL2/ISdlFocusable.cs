namespace AbstUI.SDL2;

public interface ISdlFocusable
{
    bool HasFocus { get; }
    void SetFocus(bool focus);
}
