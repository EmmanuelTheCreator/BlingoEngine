using System.Reflection;

namespace AbstUI.Commands;

public static class AbstCommandManagerExtensions
{
    private static List<Assembly> _loadedAssemblies = new();
    public static void DiscoverAndSubscribe(this IAbstCommandManager manager, IServiceProvider provider,params Assembly[] assemblies)
    {
        List<Assembly> assembliesList = assemblies.Where(x => !_loadedAssemblies.Contains(x)).ToList();
        var thisAssembly = Assembly.GetExecutingAssembly();
        if (_loadedAssemblies.Count == 0 && !assembliesList.Contains(thisAssembly))
            assembliesList.Add(Assembly.GetExecutingAssembly());
        
        foreach (var type in assemblies.SelectMany(a =>
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }))
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAbstCommandHandler<>)))
            {
                manager.Register(type);
            }
        }
        _loadedAssemblies.AddRange(assembliesList);
    }
}
