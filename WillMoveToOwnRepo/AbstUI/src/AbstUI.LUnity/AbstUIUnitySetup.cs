using AbstUI.Components;
using AbstUI.LUnity.Components;
using AbstUI.LUnity.Styles;
using AbstUI.LUnity.Windowing;
using AbstUI.Inputs;
using AbstUI.LUnity.Inputs;
using AbstUI.Styles;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.LUnity
{
    public static class AbstUIUnitySetup
    {
        public static IServiceCollection WithAbstUIUnity(this IServiceCollection services)
        {
            services
                .AddSingleton<IAbstFontManager, UnityFontManager>()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstComponentFactory, AbstUnityComponentFactory>()
                .AddSingleton<IAbstFrameworkWindowManager, AbstUnityWindowManager>()
                .AddSingleton<IAbstFrameworkMainWindow, AbstUnityMainWindow>()
                .AddTransient<IAbstFrameworkDialog, AbstUnityDialog>()
                .AddSingleton<IAbstGlobalMouse, GlobalUnityAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalUnityAbstKey>()
                .WithAbstUI();

            return services;
        }
        public static IServiceProvider WithAbstUIUnity(this IServiceProvider services)
        {
            services.WithAbstUI(); // need to be first to register all the windows in the windows factory.

            var factory = services.GetRequiredService<IAbstComponentFactory>();
            factory
                .Register<AbstMouse, AbstUnityMouse>()
                .Register<GlobalUnityAbstMouse, AbstUnityGlobalMouse>()
                .Register<AbstKey, AbstUIUnityKey>()
                .Register<AbstDialog, AbstUnityDialog>()
                .Register<AbstMainWindow, AbstUnityMainWindow>()
                .Register<AbstWindowManager, AbstUnityWindowManager>();

            return services;
        }
    }
}
