using BlingoEngine.Core;
using Microsoft.Extensions.DependencyInjection;
namespace BlingoEngine.Setup
{
    public static class BlingoEngineRegistrationExtensions
    {
        public static IServiceCollection RegisterBlingoEngine(this IServiceCollection container, Action<IBlingoEngineRegistration> config)
        {
            var blingoServiceProvider = new BlingoServiceProvider();
            container.AddSingleton<IBlingoServiceProvider>(blingoServiceProvider);
            var engineRegistration = new BlingoEngineRegistration(container, blingoServiceProvider);
            engineRegistration.RegisterCommonServices();
            container.AddSingleton<IBlingoEngineRegistration>(engineRegistration);
            config(engineRegistration);
           
            return container;
        }
    }
}


