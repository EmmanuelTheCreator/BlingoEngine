using System;
using AbstUI.Components.Buttons;
using AbstUI.Components.Menus;
using AbstUI.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Menus;

internal class AbstBlazorMenu : IAbstFrameworkMenu, IFrameworkFor<AbstMenu>, IDisposable
{
    public string Name { get; set; } = string.Empty;
    public bool Visibility { get; set; } = true;
    public float Width { get; set; }
    public float Height { get; set; }
    public AMargin Margin { get; set; } = AMargin.Zero;
    public float X { get; set; }
    public float Y { get; set; }
    public object FrameworkNode => this;

    public int ZIndex { get; set; }

    public AbstBlazorMenu(AbstBlazorComponentFactory factory, string name)
    {
        Name = name;
    }

    public void AddItem(IAbstFrameworkMenuItem item) { }
    public void ClearItems() { }
    public void PositionPopup(IAbstFrameworkButton button) { }
    public void Popup() { }

    public void Dispose() { }
}
