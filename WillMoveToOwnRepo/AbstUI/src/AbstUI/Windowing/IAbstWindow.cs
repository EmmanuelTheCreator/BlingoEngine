using AbstUI.Inputs;
using AbstUI.Components;

namespace AbstUI.Windowing;

public interface IAbstWindow : IAbstMouseRectProvider
{
    public string WindowCode { get; }
    public string Title { get; }
    public int Width { get; }
    public int Height { get; }
    public int X { get; }
    public int Y { get; }
    public int MinimumWidth { get; }
    public int MinimumHeight { get; }
    IAbstMouse Mouse { get; }
    IAbstKey Key { get; }
    bool IsOpen { get; }

    IAbstFrameworkWindow FrameworkObj { get; }
    int WindowTitleHeight { get; set; }
    IAbstNode? Content { get; set; }

    void Init(IAbstFrameworkWindow frameworkWindow);
    void OpenWindow();
    void CloseWindow();
    void MoveWindow(int x, int y);
    void SetPositionAndSize(int x, int y, int width, int height);
    void SetSize(int width, int height);
    void SetActivated(bool state);
}

