using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Containers;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkTabContainer"/>.
/// </summary>
internal class AbstUnityTabContainer : AbstUnityComponent, IAbstFrameworkTabContainer, IFrameworkFor<AbstTabContainer>
{
    private readonly List<IAbstFrameworkTabItem> _tabs = new();
    private int _selectedIndex = -1;

    public AbstUnityTabContainer() : base(CreateGameObject())
    {
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("TabContainer", typeof(RectTransform));
        go.AddComponent<Image>();
        return go;
    }

    public string? SelectedTabName =>
        _selectedIndex >= 0 && _selectedIndex < _tabs.Count ? _tabs[_selectedIndex].Title : null;

    public void AddTab(IAbstFrameworkTabItem content)
    {
        _tabs.Add(content);
        if (content.FrameworkNode is GameObject go)
            go.transform.parent = GameObject.transform;
        if (_selectedIndex == -1)
            _selectedIndex = 0;
    }

    public void RemoveTab(IAbstFrameworkTabItem content)
    {
        var idx = _tabs.IndexOf(content);
        if (idx >= 0)
        {
            _tabs.RemoveAt(idx);
            if (content.FrameworkNode is GameObject go)
                go.transform.parent = null;
            if (_selectedIndex >= _tabs.Count)
                _selectedIndex = _tabs.Count - 1;
        }
    }

    public IEnumerable<IAbstFrameworkTabItem> GetTabs() => _tabs.ToArray();

    public void ClearTabs()
    {
        foreach (var tab in _tabs.ToArray())
            RemoveTab(tab);
        _tabs.Clear();
        _selectedIndex = -1;
    }

    public void SelectTabByName(string tabName)
    {
        var idx = _tabs.FindIndex(t => t.Title == tabName);
        if (idx >= 0)
            _selectedIndex = idx;
    }
}

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkTabItem"/>.
/// </summary>
internal class AbstUnityTabItem : AbstUnityComponent, IAbstFrameworkTabItem, IFrameworkFor<AbstTabItem>
{
    private IAbstNode? _content;

    public AbstUnityTabItem() : base(CreateGameObject())
    {
    }

    private static GameObject CreateGameObject()
    {
        var go = new GameObject("TabItem", typeof(RectTransform));
        go.AddComponent<Image>();
        return go;
    }

    public string Title { get; set; } = string.Empty;

    public IAbstNode? Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            if (_content?.FrameworkObj.FrameworkNode is GameObject oldGo)
                oldGo.transform.parent = null;
            _content = value;
            if (_content?.FrameworkObj.FrameworkNode is GameObject newGo)
                newGo.transform.parent = GameObject.transform;
        }
    }

    public float TopHeight { get; set; }
}
