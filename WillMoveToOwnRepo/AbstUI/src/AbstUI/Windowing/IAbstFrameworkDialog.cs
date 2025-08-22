using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Windowing
{
    public interface IAbstFrameworkDialog<T> : IAbstFrameworkDialog, IFrameworkFor<T>
        where T: IAbstDialog
    {

    }
    public interface IAbstFrameworkDialog : IAbstFrameworkLayoutNode
    {
        /// <summary>Window title.</summary>
        string Title { get; set; }
        AColor BackgroundColor { get; set; }


        bool IsOpen { get; }
        bool IsPopup { get; set; }
        bool Borderless { get; set; }
        bool IsActiveWindow { get; }

        IAbstMouse Mouse { get; }
        IAbstKey Key { get; }


        event Action<bool>? OnWindowStateChanged;

        

        void SetPositionAndSize(int x, int y, int width, int height);
        APoint GetPosition();
        APoint GetSize();
        void SetSize(int width, int height);
        void AddItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode);
        void RemoveItem(IAbstFrameworkLayoutNode abstFrameworkLayoutNode);
        IEnumerable<IAbstFrameworkLayoutNode> GetItems();
        void Popup();
        void PopupCentered();
        void Hide();
    }
}
