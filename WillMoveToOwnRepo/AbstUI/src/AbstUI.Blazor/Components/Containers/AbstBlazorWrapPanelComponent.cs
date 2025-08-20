using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components;

public class AbstBlazorWrapPanelComponent : AbstBlazorComponentModelBase, IAbstFrameworkWrapPanel, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry = default!;
    private AbstBlazorComponentContainer _children = default!;
    private readonly List<IAbstFrameworkNode> _items = new();

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        _registry = registry;
        _children = new AbstBlazorComponentContainer(registry.Mapper);
    }

    public AbstBlazorComponentContainer ChildContainer => _children;

    private AOrientation _orientation;
    public AOrientation Orientation
    {
        get => _orientation;
        set { if (_orientation != value) { _orientation = value; RaiseChanged(); } }
    }

    private APoint _itemMargin;
    public APoint ItemMargin
    {
        get => _itemMargin;
        set { if (!_itemMargin.Equals(value)) { _itemMargin = value; RaiseChanged(); } }
    }

    public void AddItem(IAbstFrameworkNode child)
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

    public void RemoveItem(IAbstFrameworkNode child)
    {
        if (_items.Remove(child))
        {
            _children.Unregister(child);
            _registry.Unregister(child);
            RaiseChanged();
        }
    }

    public IEnumerable<IAbstFrameworkNode> GetItems() => _items;

    public IAbstFrameworkNode? GetItem(int index) => index >= 0 && index < _items.Count ? _items[index] : null;

    public void RemoveAll()
    {
        foreach (var item in _items)
        {
            _children.Unregister(item);
            _registry.Unregister(item);
        }
        _items.Clear();
        RaiseChanged();
    }
}
