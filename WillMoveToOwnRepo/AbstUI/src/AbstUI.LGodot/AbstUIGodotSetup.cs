using AbstEngine.Director.LGodot.Windowing;
using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using LingoEngine.LGodot;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.LGodot
{
    public static class AbstUIGodotSetup
    {
       
        public static IServiceCollection WithAbstUIGodot(this IServiceCollection services)
        {
            services
                .AddSingleton<GodotComponentFactory>()
                .AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<GodotComponentFactory>())
                .AddTransient<IAbstFrameworkDialog, AbstGodotDialog>()

                .AddSingleton<IAbstGlobalMouse, GlobalGodotAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalAbstKey>()
                .AddSingleton<IAbstGodotWindowManager, AbstGodotWindowManager>()


                .AddSingleton<IAbstFontManager, AbstGodotFontManager>()
                .AddSingleton<IAbstStyleManager, AbstGodotStyleManager>()
                .AddTransient(p => (AbstGodotFontManager)p.GetRequiredService<IAbstFontManager>())
                .AddTransient(p => (AbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .AddTransient(p => (IAbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .WithAbstUI()
                ;

            return services;
        }

        public static IServiceProvider WithAbstUIGodot(this IServiceProvider services)
        {
            //Console.WriteLine("AbstUIGodotSetup: Is this still needed?");
            services.GetRequiredService<IAbstComponentFactory>()
                .DiscoverInAssembly(typeof(AbstUIGodotSetup).Assembly)
                ;
            //services.GetRequiredService<IAbstComponentFactory>()
            //     .Register<IAbstDialog,AbstGodotDialog>()
            //    ;
            return services;
        }
    }
}
