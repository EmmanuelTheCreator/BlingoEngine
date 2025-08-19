using System;
using System.Collections.Generic;

namespace AbstUI.Blazor;

/// <summary>
/// Maps abstract framework component types to their Razor component counterparts.
/// </summary>
public class AbstBlazorComponentMapper
{
    private readonly Dictionary<Type, Type> _typeMappings = new();

    /// <summary>Registers a mapping between an abstract component type and a Razor component.</summary>
    public void Map<TComponent>(Type razorComponentType) => _typeMappings[typeof(TComponent)] = razorComponentType;

    /// <summary>Gets the Razor component type mapped to the given framework component instance.</summary>
    public Type GetRazorType(object component) => GetRazorType(component.GetType());

    /// <summary>Gets the Razor component type mapped to the given framework component type.</summary>
    public Type GetRazorType(Type componentType)
    {
        if (_typeMappings.TryGetValue(componentType, out var razorType))
            return razorType;
        throw new InvalidOperationException($"No Razor component mapped for {componentType.Name}");
    }
}

