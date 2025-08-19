namespace AbstUI.Windowing;

public interface IAbstWindow
{
    void Init(IAbstFrameworkWindow frameworkWindow);
    bool IsOpen { get; }
    void OpenWindow();
    void CloseWindow();
    void MoveWindow(int x, int y);
    void SetPositionAndSize(int x, int y, int width, int height);
    void SetSize(int width, int height);
    IAbstFrameworkWindow FrameworkObj { get; }
}
