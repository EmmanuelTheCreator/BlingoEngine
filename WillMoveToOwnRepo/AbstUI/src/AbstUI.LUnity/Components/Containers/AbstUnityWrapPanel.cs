using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkWrapPanel"/>.
/// </summary>
internal class AbstUnityWrapPanel : AbstUnityComponent, IAbstFrameworkWrapPanel
{
    private readonly List<IAbstFrameworkNode> _children = new();
    private readonly HorizontalOrVerticalLayoutGroup _layout;
    private APoint _itemMargin;

    public AbstUnityWrapPanel(AOrientation orientation) : base(CreateGameObject(orientation, out var layout))
    {
        Orientation = orientation;
        _layout = layout;
        ItemMargin = new APoint(0, 0);
    }

    private static GameObject CreateGameObject(AOrientation orientation, out HorizontalOrVerticalLayoutGroup layout)
    {
        var go = new GameObject("WrapPanel", typeof(RectTransform));
        layout = orientation == AOrientation.Horizontal
            ? go.AddComponent<HorizontalLayoutGroup>() as HorizontalOrVerticalLayoutGroup
            : go.AddComponent<VerticalLayoutGroup>() as HorizontalOrVerticalLayoutGroup;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        layout.spacing = 0f;
        return go;
    }

    public AOrientation Orientation { get; set; }

    public APoint ItemMargin
    {
        get => _itemMargin;
        set
        {
            _itemMargin = value;
            _layout.spacing = Orientation == AOrientation.Horizontal ? value.X : value.Y;
        }
    }

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

