using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.FrameworkCommunication;
using AbstUI.LUnity.Components.Base;
using AbstUI.LUnity.Primitives;
using AbstUI.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Containers;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkPanel"/>.
/// </summary>
internal class AbstUnityPanel : AbstUnityComponent, IAbstFrameworkPanel, IFrameworkFor<AbstPanel>
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();
    private readonly Image _image;
    private readonly Outline _outline;
    private AColor? _backgroundColor;
    private AColor? _borderColor;
    private float _borderWidth;

    public AbstUnityPanel() : base(CreateGameObject(out var image, out var outline))
    {
        _image = image;
        _outline = outline;
    }

    private static GameObject CreateGameObject(out Image image, out Outline outline)
    {
        var go = new GameObject("Panel", typeof(RectTransform));
        image = go.AddComponent<Image>();
        outline = go.AddComponent<Outline>();
        return go;
    }

    public AColor? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            _image.color = value.HasValue
                ? value.Value.ToUnityColor()
                : Color.clear;
        }
    }

    public AColor? BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            _outline.effectColor = value.HasValue
                ? value.Value.ToUnityColor()
                : Color.clear;
        }
    }

    public float BorderWidth
    {
        get => _borderWidth;
        set
        {
            _borderWidth = value;
            _outline.effectDistance = new Vector2(value, value);
        }
    }

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
