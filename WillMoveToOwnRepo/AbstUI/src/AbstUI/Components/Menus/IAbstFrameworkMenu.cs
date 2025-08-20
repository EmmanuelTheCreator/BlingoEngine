using AbstUI.Components.Buttons;
using System.Collections.Generic;

namespace AbstUI.Components.Menus
{
    /// <summary>
    /// Framework specific menu container capable of holding menu items.
    /// </summary>
    public interface IAbstFrameworkMenu : IAbstFrameworkLayoutNode
    {
        /// <summary>Adds an item to the menu.</summary>
        void AddItem(IAbstFrameworkMenuItem item);
        /// <summary>Removes all items from the menu.</summary>
        void ClearItems();
        /// <summary>Positions the popup relative to a button.</summary>
        void PositionPopup(IAbstFrameworkButton button);
        /// <summary>Shows the menu.</summary>
        void Popup();
    }
}
