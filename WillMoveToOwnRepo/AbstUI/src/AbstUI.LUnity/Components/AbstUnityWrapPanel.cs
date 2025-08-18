using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;
using UnityEngine;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkWrapPanel"/>.
/// </summary>
internal class AbstUnityWrapPanel : AbstUnityComponent, IAbstFrameworkWrapPanel
{
    private readonly List<IAbstFrameworkNode> _children = new();

    public AbstUnityWrapPanel(AOrientation orientation)
    {
        Orientation = orientation;
        ItemMargin = new APoint(0, 0);
    }

    public AOrientation Orientation { get; set; }

    public APoint ItemMargin { get; set; }

    public void AddItem(IAbstFrameworkNode child)
    {
        _children.Add(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = GameObject.transform;
    }

    public void RemoveItem(IAbstFrameworkNode child)
    {
        _children.Remove(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = null;
    }

    public IEnumerable<IAbstFrameworkNode> GetItems() => _children;

    public IAbstFrameworkNode? GetItem(int index)
        => index >= 0 && index < _children.Count ? _children[index] : null;

    public void RemoveAll()
    {
        foreach (var child in _children.ToArray())
            RemoveItem(child);
    }
}

