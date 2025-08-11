using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Commands;

internal static class CommandManagerExtensions
{
    public static void DiscoverAndSubscribe(this ILingoCommandManager manager, IServiceProvider provider)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x =>
        {
            var name1 = x.GetName().Name;
            return name1 != null && !name1!.StartsWith("System") && !name1!.StartsWith("Microsoft");
        });
        foreach (var type in assemblies.SelectMany(a =>
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }))
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                manager.Register(type);
            }
        }
    }
}
