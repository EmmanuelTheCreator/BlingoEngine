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
                .AddSingleton<IAbstFrameworkMainWindow, AbstUnityMainWindow>()
                .WithAbstUI();

            return services;
        }
    }
}
