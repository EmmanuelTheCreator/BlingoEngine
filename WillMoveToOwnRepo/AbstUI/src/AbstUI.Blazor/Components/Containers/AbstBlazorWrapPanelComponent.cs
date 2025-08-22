using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.FrameworkCommunication;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorWrapPanelComponent : AbstBlazorComponentModelBase, IAbstFrameworkWrapPanel, IFrameworkFor<AbstWrapPanel>, IAbstBlazorRegistryAware
{
    private AbstBlazorComponentContainer _registry;
    private readonly AbstBlazorComponentContainer _children;
    private readonly List<IAbstFrameworkNode> _items = new();

    public AbstBlazorWrapPanelComponent(AbstBlazorComponentContainer registry, AbstBlazorComponentMapper mapper)
    {
        _registry = registry;
        _children = new AbstBlazorComponentContainer(mapper);
    }

    public void AttachRegistry(AbstBlazorComponentContainer registry)
    {
        if (!ReferenceEquals(_registry, registry))
        {
            _registry = registry;
            foreach (var item in _items)
            {
                if (item is IAbstBlazorRegistryAware aware)
                    aware.AttachRegistry(_registry);
                _registry.Register(item);
            }
        }
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
