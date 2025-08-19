using System.Collections.Generic;
using AbstUI.Components;

namespace AbstUI.Blazor.Components;

public class AbstBlazorTabContainerComponent : AbstBlazorComponentModelBase, IAbstFrameworkTabContainer, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry = default!;
    private AbstBlazorComponentContainer _children = default!;
    private readonly List<AbstBlazorTabItemComponent> _tabs = new();
    private int _selectedIndex = -1;

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        _registry = registry;
        _children = new AbstBlazorComponentContainer(registry.Mapper);
    }

    public AbstBlazorComponentContainer ChildContainer => _children;
    internal IReadOnlyList<AbstBlazorTabItemComponent> Tabs => _tabs;
    internal int SelectedIndex => _selectedIndex;
    internal object? ActiveContentNode => _selectedIndex >= 0 && _selectedIndex < _tabs.Count ? _tabs[_selectedIndex].ContentFrameworkNode : null;

    public string SelectedTabName => _selectedIndex >= 0 && _selectedIndex < _tabs.Count ? _tabs[_selectedIndex].Title : string.Empty;

    public void AddTab(IAbstFrameworkTabItem content)
    {
        var tab = (AbstBlazorTabItemComponent)content;
        _tabs.Add(tab);
        if (tab.ContentFrameworkNode is { } node)
        {
            if (node is IAbstBlazorRegistryAware aware)
                aware.AttachRegistry(_registry);
            _registry.Register(node);
            _children.Register(node);
        }
        if (_selectedIndex == -1)
            _selectedIndex = 0;
        RaiseChanged();
    }

    public void RemoveTab(IAbstFrameworkTabItem content)
    {
        var tab = (AbstBlazorTabItemComponent)content;
        var index = _tabs.IndexOf(tab);
        if (index >= 0)
        {
            if (tab.ContentFrameworkNode is { } node)
            {
                _children.Unregister(node);
                _registry.Unregister(node);
            }
            _tabs.RemoveAt(index);
            if (_selectedIndex >= _tabs.Count)
                _selectedIndex = _tabs.Count - 1;
            RaiseChanged();
        }
    }

    public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _tabs;

    public void ClearTabs()
    {
        foreach (var tab in _tabs)
            if (tab.ContentFrameworkNode is { } node)
            {
                _children.Unregister(node);
                _registry.Unregister(node);
            }
        _tabs.Clear();
        _selectedIndex = -1;
        RaiseChanged();
    }

    public void SelectTabByName(string tabName)
    {
        var idx = _tabs.FindIndex(t => t.Title == tabName);
        if (idx >= 0)
        {
            _selectedIndex = idx;
            RaiseChanged();
        }
    }

    internal void SelectTab(int index)
    {
        if (index >= 0 && index < _tabs.Count)
        {
            _selectedIndex = index;
            RaiseChanged();
        }
    }
}
