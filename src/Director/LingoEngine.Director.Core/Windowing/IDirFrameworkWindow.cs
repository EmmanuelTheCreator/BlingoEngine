
using AbstUI.Primitives;
using LingoEngine.Inputs;

namespace LingoEngine.Director.Core.Windowing
{
    public interface IDirFrameworkWindow
    {
        bool IsOpen { get; }
        bool IsActiveWindow { get; }
        ILingoMouse Mouse { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
        void SetPositionAndSize(int x, int y, int width, int height);
        APoint GetPosition();
        APoint GetSize();
        void SetSize(int width, int height);
    }
}
