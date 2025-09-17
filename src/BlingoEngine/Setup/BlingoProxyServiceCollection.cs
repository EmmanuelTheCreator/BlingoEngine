using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Setup;

internal class BlingoProxyServiceCollection : IServiceCollection
{
    private readonly IServiceCollection _inner;
    private readonly List<ServiceDescriptor> _registrations = new();

    public BlingoProxyServiceCollection(IServiceCollection inner)
    {
        _inner = inner;
    }

    public ServiceDescriptor this[int index]
    {
        get => _inner[index];
        set
        {
            _inner[index] = value;
            _registrations.Add(value);
        }
    }

    public int Count => _inner.Count;

    public bool IsReadOnly => _inner.IsReadOnly;

    public void Add(ServiceDescriptor item)
    {
        _inner.Add(item);
        _registrations.Add(item);
    }

    public void Clear()
    {
        foreach (var d in _registrations)
            _inner.Remove(d);
        _registrations.Clear();
    }

    public bool Contains(ServiceDescriptor item) => _inner.Contains(item);

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

    public IEnumerator<ServiceDescriptor> GetEnumerator() => _inner.GetEnumerator();

    public int IndexOf(ServiceDescriptor item) => _inner.IndexOf(item);

    public void Insert(int index, ServiceDescriptor item)
    {
        _inner.Insert(index, item);
        _registrations.Add(item);
    }

    public bool Remove(ServiceDescriptor item)
    {
        var removed = _inner.Remove(item);
        if (removed)
            _registrations.Remove(item);
        return removed;
    }

    public void RemoveAt(int index)
    {
        var descriptor = _inner[index];
        _inner.RemoveAt(index);
        _registrations.Remove(descriptor);
    }

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public void UnregisterMovie(string? preserveNamespaceFragment = null)
    {
        for (int i = _registrations.Count - 1; i >= 0; i--)
        {
            var d = _registrations[i];
            if (preserveNamespaceFragment != null && HasNamespace(d, preserveNamespaceFragment))
                continue;
            _inner.Remove(d);
            _registrations.RemoveAt(i);
        }
    }

    private static bool HasNamespace(ServiceDescriptor descriptor, string fragment)
    {
        var ns = descriptor.ServiceType?.Namespace;
        if (ns != null && ns.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        ns = descriptor.ImplementationType?.Namespace;
        if (ns != null && ns.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        var instType = descriptor.ImplementationInstance?.GetType();
        ns = instType?.Namespace;
        return ns != null && ns.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

