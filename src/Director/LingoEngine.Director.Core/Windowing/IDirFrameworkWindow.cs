
namespace LingoEngine.Director.Core.Windowing
{
    public interface IDirFrameworkWindow
    {
        bool IsOpen { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
        void SetPositionAndSize(int x, int y, int width, int height);

    }
}
