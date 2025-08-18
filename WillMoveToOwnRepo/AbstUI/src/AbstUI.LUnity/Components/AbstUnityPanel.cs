using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkPanel"/>.
/// </summary>
internal class AbstUnityPanel : AbstUnityComponent, IAbstFrameworkPanel
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();

    public AColor? BackgroundColor { get; set; }
    public AColor? BorderColor { get; set; }
    public float BorderWidth { get; set; }

    public void AddItem(IAbstFrameworkLayoutNode child)
    {
        _children.Add(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = GameObject.transform;
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children;

    public void RemoveAll()
    {
        foreach (var child in _children.ToArray())
            RemoveItem(child);
    }

    public void RemoveItem(IAbstFrameworkLayoutNode child)
    {
        _children.Remove(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = null;
    }
}
