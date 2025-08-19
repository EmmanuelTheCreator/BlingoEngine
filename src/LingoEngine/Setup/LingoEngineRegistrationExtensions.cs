using AbstUI.Core;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.Setup
{
    public static class LingoEngineRegistrationExtensions
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container, Action<ILingoEngineRegistration> config)
        {
            var lingoServiceProvider = new AbstServiceProvider();
            container.AddSingleton<IAbstServiceProvider>(lingoServiceProvider);
            var engineRegistration = new LingoEngineRegistration(container, lingoServiceProvider);
            engineRegistration.RegisterCommonServices();
            container.AddSingleton<ILingoEngineRegistration>(engineRegistration);
            config(engineRegistration);
            return container;
        }
    }
}

