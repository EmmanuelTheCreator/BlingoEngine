using System;
using System.Collections.Generic;

namespace AbstUI.Blazor;

/// <summary>
/// Stores framework components for a particular branch of the visual tree and
/// resolves their corresponding Razor component types through the mapper.
/// </summary>
public class AbstBlazorComponentContainer
{
    private readonly AbstBlazorComponentMapper _mapper;
    private readonly List<object> _components = new();

    public AbstBlazorComponentContainer(AbstBlazorComponentMapper mapper)
    {
        _mapper = mapper;
    }

    public AbstBlazorComponentMapper Mapper => _mapper;

    /// <summary>Components registered with this container in insertion order.</summary>
    public IReadOnlyList<object> Components => _components;

    public void Register(object component) => _components.Add(component);

    public void Unregister(object component) => _components.Remove(component);

    public Type GetRazorComponentType(object component) => _mapper.GetRazorType(component);
}

