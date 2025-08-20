using System;
using System.Collections.Generic;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Primitives;

namespace AbstUI.Blazor.Components.Containers;

public class AbstBlazorPanelComponent : AbstBlazorComponentModelBase, IAbstFrameworkPanel, IAbstBlazorRegistryAware
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

    private AColor? _backgroundColor;
    public AColor? BackgroundColor
    {
        get => _backgroundColor;
        set { if (!Nullable.Equals(_backgroundColor, value)) { _backgroundColor = value; RaiseChanged(); } }
    }

    private AColor? _borderColor;
    public AColor? BorderColor
    {
        get => _borderColor;
        set { if (!Nullable.Equals(_borderColor, value)) { _borderColor = value; RaiseChanged(); } }
    }

    private float _borderWidth;
    public float BorderWidth
    {
        get => _borderWidth;
        set { if (Math.Abs(_borderWidth - value) > float.Epsilon) { _borderWidth = value; RaiseChanged(); } }
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

    public void RemoveAll()
    {
        foreach (var c in _items)
        {
            _children.Unregister(c);
            _registry.Unregister(c);
        }
        _items.Clear();
        RaiseChanged();
    }

    public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _items;
}
