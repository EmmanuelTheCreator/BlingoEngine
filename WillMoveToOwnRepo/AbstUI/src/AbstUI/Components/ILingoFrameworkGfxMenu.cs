using System.Collections.Generic;

namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific menu container capable of holding menu items.
    /// </summary>
    public interface IAbstUIFrameworkGfxMenu : IAbstUIFrameworkGfxLayoutNode
    {
        /// <summary>Adds an item to the menu.</summary>
        void AddItem(IAbstUIFrameworkGfxMenuItem item);
        /// <summary>Removes all items from the menu.</summary>
        void ClearItems();
        /// <summary>Positions the popup relative to a button.</summary>
        void PositionPopup(IAbstUIFrameworkGfxButton button);
        /// <summary>Shows the menu.</summary>
        void Popup();
    }
}
