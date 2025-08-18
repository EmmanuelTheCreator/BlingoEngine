using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using AbstUI.Components;

namespace AbstUI.Blazor.Components;

public partial class AbstBlazorTabContainer : AbstBlazorComponentBase, IAbstFrameworkTabContainer
{
    private readonly List<IAbstFrameworkTabItem> _tabs = new();
    private readonly List<RenderFragment> _fragments = new();
    private int _selectedIndex = -1;

    public string SelectedTabName => _selectedIndex >= 0 && _selectedIndex < _tabs.Count ? _tabs[_selectedIndex].Title : string.Empty;

    public void AddTab(IAbstFrameworkTabItem content)
    {
        _tabs.Add(content);
        if (content is AbstBlazorTabItem item && item.ContentFragment != null)
            _fragments.Add(item.ContentFragment);
        else
            _fragments.Add(builder => { });
        if (_selectedIndex == -1) _selectedIndex = 0;
        RequestRender();
    }

    public void RemoveTab(IAbstFrameworkTabItem content)
    {
        var index = _tabs.IndexOf(content);
        if (index >= 0)
        {
            _tabs.RemoveAt(index);
            _fragments.RemoveAt(index);
            if (_selectedIndex >= _tabs.Count) _selectedIndex = _tabs.Count - 1;
            RequestRender();
        }
    }

    public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _tabs;

    public void ClearTabs()
    {
        _tabs.Clear();
        _fragments.Clear();
        _selectedIndex = -1;
        RequestRender();
    }

    public void SelectTabByName(string tabName)
    {
        var idx = _tabs.FindIndex(t => t.Title == tabName);
        if (idx >= 0)
        {
            _selectedIndex = idx;
            RequestRender();
        }
    }

    private void SelectTab(int index)
    {
        _selectedIndex = index;
    }

    internal IReadOnlyList<IAbstFrameworkTabItem> Tabs => _tabs;
    internal RenderFragment? ActiveContent => _selectedIndex >= 0 && _selectedIndex < _fragments.Count ? _fragments[_selectedIndex] : null;
}
