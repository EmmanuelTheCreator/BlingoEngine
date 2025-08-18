using System.Collections.Generic;
using AbstUI.Components;
using UnityEngine;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkScrollContainer"/>.
/// </summary>
internal class AbstUnityScrollContainer : AbstUnityComponent, IAbstFrameworkScrollContainer
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();

    public float ScrollHorizontal { get; set; }
    public float ScrollVertical { get; set; }
    public bool ClipContents { get; set; }

    public void AddItem(IAbstFrameworkLayoutNode child)
    {
        _children.Add(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = GameObject.transform;
    }

    public void RemoveItem(IAbstFrameworkLayoutNode child)
    {
        _children.Remove(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.parent = null;
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children;
}
