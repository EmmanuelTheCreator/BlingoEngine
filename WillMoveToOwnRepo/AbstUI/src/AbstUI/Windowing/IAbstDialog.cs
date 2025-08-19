using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;

namespace AbstUI.Windowing;

public interface IAbstDialog : IDisposable
{
    /// <summary>Window title.</summary>
    string Title { get; set; }
    AColor BackgroundColor { get; set; }
    bool IsPopup { get; set; }
    /// <summary>Whether the window is borderless (no title bar).</summary>
    bool Borderless { get; set; }
    bool IsOpen { get; }


    IAbstMouse Mouse { get; }
    IAbstKey Key { get; }


    /// <summary>Shows the window at its current position.</summary>
    void Popup();
    /// <summary>Centers the window on screen and shows it.</summary>
    void PopupCentered();
    /// <summary>Hides the window.</summary>
    void Hide();
    void Init(IAbstFrameworkDialog framework);

    /// <summary>Adds a child node to the window.</summary>
    IAbstDialog AddItem(IAbstFrameworkLayoutNode child);
    IAbstDialog RemoveItem(IAbstFrameworkLayoutNode child);
    IEnumerable<IAbstFrameworkLayoutNode> GetItems();
    T FrameworkObj<T>() where T: IAbstFrameworkDialog;
}
