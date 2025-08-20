using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Windowing;
using AbstUI.Styles;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.SDL2
{
    public static class AbstUISDLSetup
    {
        public static IServiceCollection WithAbstUISdl(this IServiceCollection services)
        {
            services
                .AddSingleton<IAbstSdlWindowManager, AbstSdlWindowManager>()
                .AddSingleton<IAbstFrameworkWindowManager>(p => p.GetRequiredService<IAbstSdlWindowManager>())
                .AddSingleton<AbstSdlComponentFactory>()
                .AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<AbstSdlComponentFactory>())
                //.AddTransient<IAbstFrameworkDialog, AbstSDLDialog>()


                .AddSingleton<IAbstFontManager, SdlFontManager>()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddTransient(p => (SdlFontManager)p.GetRequiredService<IAbstFontManager>())
                .AddTransient(p => (AbstStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .AddSingleton<SdlFocusManager>()


                .AddSingleton<IAbstGlobalMouse, GlobalSDLAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalSDLAbstKey>()

                .WithAbstUI();

            return services;
        }

        public static IServiceProvider WithAbstUISdl(this IServiceProvider services)
        {
            //Console.WriteLine("AbstUIGodotSetup: Is this still needed?");
            services.GetRequiredService<AbstSdlComponentFactory>().DiscoverInAssembly(typeof(AbstUISDLSetup).Assembly);
            //services.GetRequiredService<IAbstComponentFactory>()
            //     .Register<IAbstDialog,AbstGodotDialog>()
            //    ;
            return services;
        }
    }
}
