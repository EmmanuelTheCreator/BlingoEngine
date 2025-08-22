using AbstUI.Components;
using AbstUI.LUnity.Components;
using AbstUI.LUnity.Styles;
using AbstUI.LUnity.Windowing;
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
                .WithAbstUI();

            return services;
        }
        public static IServiceProvider WithAbstUIUnity(this IServiceProvider services)
        {
            services.WithAbstUI(); // need to be first to register all the windows in the windows factory.
            services.GetRequiredService<IAbstComponentFactory>()
                .DiscoverInAssembly(typeof(AbstUIUnitySetup).Assembly)
                ;
            return services;
        }
    }
}
