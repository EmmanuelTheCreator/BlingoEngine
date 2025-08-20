using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.LUnity.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace AbstUI.LUnity.Components.Containers;

/// <summary>
/// Unity implementation of <see cref="IAbstFrameworkScrollContainer"/>.
/// </summary>
internal class AbstUnityScrollContainer : AbstUnityComponent, IAbstFrameworkScrollContainer
{
    private readonly List<IAbstFrameworkLayoutNode> _children = new();
    private readonly ScrollRect _scrollRect;
    private readonly RectTransform _content;
    private readonly Mask _mask;

    public AbstUnityScrollContainer() : base(CreateGameObject(out var scrollRect, out var content, out var mask))
    {
        _scrollRect = scrollRect;
        _content = content;
        _mask = mask;
    }

    private static GameObject CreateGameObject(out ScrollRect scrollRect, out RectTransform content, out Mask mask)
    {
        var go = new GameObject("ScrollContainer", typeof(RectTransform));
        go.AddComponent<Image>();
        mask = go.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollRect = go.AddComponent<ScrollRect>();

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(go.transform, false);
        content = contentGo.GetComponent<RectTransform>();
        scrollRect.content = content;
        return go;
    }

    public float ScrollHorizontal
    {
        get => _scrollRect.horizontalNormalizedPosition;
        set => _scrollRect.horizontalNormalizedPosition = value;
    }

    public float ScrollVertical
    {
        get => _scrollRect.verticalNormalizedPosition;
        set => _scrollRect.verticalNormalizedPosition = value;
    }

    public bool ClipContents
    {
        get => _mask.enabled;
        set => _mask.enabled = value;
    }

    public void AddItem(IAbstFrameworkLayoutNode child)
    {
        _children.Add(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.SetParent(_content, false);
    }

    public void RemoveItem(IAbstFrameworkLayoutNode child)
    {
        _children.Remove(child);
        if (child.FrameworkNode is GameObject go)
            go.transform.SetParent(null);
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _children;
}
