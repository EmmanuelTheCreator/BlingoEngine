using System.Collections.Generic;
using AbstUI.Components.Buttons;
using AbstUI.Components.Menus;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using UnityEngine;

namespace AbstUI.LUnity.Components.Menus;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkMenu"/>.
/// </summary>
internal class AbstUnityMenu : AbstUnityComponent, IAbstFrameworkMenu, IFrameworkFor<AbstMenu>
{
    private readonly List<IAbstFrameworkMenuItem> _items = new();

    public AbstUnityMenu(string name) : base(new GameObject(name))
    {
    }

    public void AddItem(IAbstFrameworkMenuItem item)
    {
        _items.Add(item);
    }

    public void ClearItems()
    {
        _items.Clear();
    }

    public void PositionPopup(IAbstFrameworkButton button)
    {
        // Popup positioning is handled by Unity's layout; nothing required here.
    }

    public void Popup()
    {
        // No default popup behaviour in plain Unity; component acts as container only.
    }
}
