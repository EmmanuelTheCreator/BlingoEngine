using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Windowing
{
    /// <summary>
    /// Framework specific window container.
    /// </summary>
    public interface IAbstFrameworkWindow : IFrameworkForInitializable<IAbstWindow> , IAbstFrameworkNode
    {
        /// <summary>Window title.</summary>
        string Title { get; set; }
        AColor BackgroundColor { get; set; }

        bool IsActiveWindow { get; }
        bool IsOpen { get; }

        IAbstMouse Mouse { get; }
        IAbstKey AbstKey { get; }

        IAbstFrameworkNode? Content { get; set; }

        //IAbstMouse Mouse { get; }
        //IAbstKey Key { get; }
        void OpenWindow();
        void CloseWindow();
        void MoveWindow(int x, int y);
        void SetPositionAndSize(int x, int y, int width, int height);
        APoint GetPosition();
        APoint GetSize();
        void SetSize(int width, int height);
    }

}
