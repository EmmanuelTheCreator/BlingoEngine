using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Inputs;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.LGodot
{
    public static class AbstUIGodotSetup
    {
       
        public static IServiceCollection WithAbstUIGodot(this IServiceCollection services, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
        {
            services
                .AddSingleton<GodotComponentFactory>()
                .AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<GodotComponentFactory>())
                .AddTransient<IAbstFrameworkDialog, AbstGodotDialog>()

                .AddSingleton<IAbstGlobalMouse, GlobalGodotAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalAbstKey>()
                .AddSingleton<IAbstGodotWindowManager, AbstGodotWindowManager>()
                .AddSingleton<IAbstFrameworkMainWindow, AbstGodotMainWindow>()


                .AddSingleton<IAbstFontManager, AbstGodotFontManager>()
                .AddSingleton<IAbstStyleManager, AbstGodotStyleManager>()
                .AddTransient(p => (AbstGodotFontManager)p.GetRequiredService<IAbstFontManager>())
                .AddTransient(p => (AbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .AddTransient(p => (IAbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .WithAbstUI(windowRegistrations)
                ;

            return services;
        }

        public static IServiceProvider WithAbstUIGodot(this IServiceProvider services)
        {
            services.WithAbstUI(); // need to be first to register all the windows in the windows factory.
            services.GetRequiredService<IAbstComponentFactory>()
                .DiscoverInAssembly(typeof(AbstUIGodotSetup).Assembly)
                ;
            var windowManager = services.GetRequiredService<IAbstGodotWindowManager>();// we need to resolve the framework window manager to link him
           
            return services;
        }
    }
}
