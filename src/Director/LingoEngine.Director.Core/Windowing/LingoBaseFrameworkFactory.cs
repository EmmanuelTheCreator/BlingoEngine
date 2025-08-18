using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Windowing;

/// <summary>
/// Base factory capable of resolving framework-specific implementations
/// for given Lingo types marked with <see cref="IFrameworkFor{T}"/>.
/// </summary>
public abstract class LingoBaseFrameworkFactory
{
    private readonly ILingoServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _frameworkMap = new();

    protected LingoBaseFrameworkFactory(ILingoServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILingoServiceProvider ServiceProvider => _serviceProvider;

    /// <summary>
    /// Scans the assembly of the derived factory for types implementing
    /// <see cref="IFrameworkFor{T}"/> and caches their mapping.
    /// </summary>
    public void DiscoverInAssembly()
        => DiscoverInAssembly(GetType().Assembly);

    public void DiscoverInAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IFrameworkFor<>))
                {
                    var lingoType = iface.GenericTypeArguments[0];
                    _frameworkMap[lingoType] = type;
                }
            }
        }
    }

    /// <summary>
    /// Resolves the framework counterpart for the specified Lingo dialog.
    /// </summary>
    public TReturnType GetFrameworkFor<TReturnType>(ILingoDialog lingoDialog)
    {
        var lingoType = lingoDialog.GetType();
        if (!_frameworkMap.TryGetValue(lingoType, out var frameworkType))
            throw new InvalidOperationException($"No framework registered for {lingoType.Name}");

        var instance = _serviceProvider.GetRequiredService(frameworkType);
        return (TReturnType)instance;
    }
}
