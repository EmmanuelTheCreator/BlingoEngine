using AbstUI.Primitives;
using System;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific window container.
    /// </summary>
    public interface IAbstFrameworkWindow : IAbstFrameworkLayoutNode
    {
        /// <summary>Window title.</summary>
        string Title { get; set; }
        AColor BackgroundColor { get; set; }
        bool IsPopup { get; set; }
        /// <summary>Whether the window is borderless (no title bar).</summary>
        bool Borderless { get; set; }


        /// <summary>Adds a child node to the window.</summary>
        void AddItem(IAbstFrameworkLayoutNode child);
        void RemoveItem(IAbstFrameworkLayoutNode child);
        IEnumerable<IAbstFrameworkLayoutNode> GetItems();

        /// <summary>Shows the window at its current position.</summary>
        void Popup();
        /// <summary>Centers the window on screen and shows it.</summary>
        void PopupCentered();
        /// <summary>Hides the window.</summary>
        void Hide();
    }
}
