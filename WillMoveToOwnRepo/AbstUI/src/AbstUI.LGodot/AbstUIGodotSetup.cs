using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Texts;
using AbstUI.Inputs;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Components.Inputs;
using AbstUI.LGodot.Components.Menus;
using AbstUI.LGodot.Inputs;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Resources;
using AbstUI.LGodot.Resources;
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
                .AddSingleton<IAbstResourceManager, AbstGodotResourceManager>()
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
            var factory = services.GetRequiredService<IAbstComponentFactory>();
            factory

                .Register<AbstMouse, AbstGodotMouse>()
                .Register<GlobalGodotAbstMouse, AbstGodotGlobalMouse>()
                .Register<AbstKey, AbstGodotKey>()
                .Register<AbstDialog, AbstGodotDialog>()
                .Register<AbstMainWindow, AbstGodotMainWindow>()
                .Register<AbstWindowManager, AbstGodotWindowManager>()
                ;

            var windowManager = services.GetRequiredService<IAbstGodotWindowManager>();// we need to resolve the framework window manager to link him

            return services;
        }
    }
}
