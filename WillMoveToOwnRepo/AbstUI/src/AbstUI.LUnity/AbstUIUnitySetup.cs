using AbstUI.LUnity.Styles;
using AbstUI.Styles;
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
                .WithAbstUI();

            return services;
        }
    }
}
