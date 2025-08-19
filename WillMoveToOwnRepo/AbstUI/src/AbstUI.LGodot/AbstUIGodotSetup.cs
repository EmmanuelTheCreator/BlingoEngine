using AbstUI.LGodot.Styles;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.LGodot
{
    public static class AbstUIGodotSetup
    {
        public static IServiceCollection WithAbstUIGodot(this IServiceCollection services)
        {
            services
                .AddSingleton<IAbstFontManager, AbstGodotFontManager>()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstGodotStyleManager, AbstGodotStyleManager>();

            return services;
        }
    }
}
