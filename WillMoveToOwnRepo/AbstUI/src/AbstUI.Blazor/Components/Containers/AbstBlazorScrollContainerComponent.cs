using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;

namespace AbstUI.Blazor.Components;

public class AbstBlazorScrollContainerComponent : AbstBlazorComponentModelBase, IAbstFrameworkScrollContainer, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry = default!;
    private AbstBlazorComponentContainer _children = default!;
    private readonly List<IAbstFrameworkLayoutNode> _items = new();

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        _registry = registry;
        _children = new AbstBlazorComponentContainer(registry.Mapper);
    }

    public AbstBlazorComponentContainer ChildContainer => _children;

    private bool _clipContents;
    public bool ClipContents
    {
        get => _clipContents;
        set { if (_clipContents != value) { _clipContents = value; RaiseChanged(); } }
    }

    private float _scrollHorizontal;
    public float ScrollHorizontal
    {
        get => _scrollHorizontal;
        set { if (Math.Abs(_scrollHorizontal - value) > float.Epsilon) { _scrollHorizontal = value; RaiseChanged(); } }
    }

    private float _scrollVertical;
    public float ScrollVertical
    {
        get => _scrollVertical;
        set { if (Math.Abs(_scrollVertical - value) > float.Epsilon) { _scrollVertical = value; RaiseChanged(); } }
    }

    public void AddItem(IAbstFrameworkLayoutNode child)
    {
        if (!_items.Contains(child))
        {
            _items.Add(child);
            if (child is IAbstBlazorRegistryAware aware)
                aware.AttachRegistry(_registry);
            _registry.Register(child);
            _children.Register(child);
            RaiseChanged();
        }
    }

    public void RemoveItem(IAbstFrameworkLayoutNode child)
    {
        if (_items.Remove(child))
        {
            _children.Unregister(child);
            _registry.Unregister(child);
            RaiseChanged();
        }
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _items;
}
