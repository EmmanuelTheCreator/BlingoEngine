using LingoEngine.Core;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.Setup
{
    public static class LingoEngineRegistrationExtensions
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container, Action<ILingoEngineRegistration> config)
        {
            var lingoServiceProvider = new LingoServiceProvider();
            container.AddSingleton<ILingoServiceProvider>(lingoServiceProvider);
            var engineRegistration = new LingoEngineRegistration(container, lingoServiceProvider);
            engineRegistration.RegisterCommonServices();
            container.AddSingleton<ILingoEngineRegistration>(engineRegistration);
            config(engineRegistration);
            engineRegistration.EnsureGlobalVars();
            return container;
        }
    }
}

