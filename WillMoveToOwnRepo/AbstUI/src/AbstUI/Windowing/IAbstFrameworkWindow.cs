using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Windowing;

public interface IAbstFrameworkWindow
{
    bool IsOpen { get; }
    bool IsActiveWindow { get; }
    IAbstMouse Mouse { get; }
    IAbstKey Key { get; }
    void OpenWindow();
    void CloseWindow();
    void MoveWindow(int x, int y);
    void SetPositionAndSize(int x, int y, int width, int height);
    APoint GetPosition();
    APoint GetSize();
    void SetSize(int width, int height);
}
