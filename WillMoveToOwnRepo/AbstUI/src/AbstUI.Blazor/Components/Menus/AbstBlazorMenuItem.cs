using System;
using AbstUI.Components.Menus;

namespace AbstUI.Blazor.Components.Menus;

internal class AbstBlazorMenuItem : IAbstFrameworkMenuItem, IDisposable
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool CheckMark { get; set; }
    public string? Shortcut { get; set; }
    public event Action? Activated;

    public AbstBlazorMenuItem(AbstBlazorComponentFactory factory, string name, string? shortcut)
    {
        Name = name;
        Shortcut = shortcut;
    }

    public void Invoke() => Activated?.Invoke();

    public void Dispose() { }
}
