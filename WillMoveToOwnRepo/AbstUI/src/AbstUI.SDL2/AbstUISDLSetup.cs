using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Inputs;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Windowing;
using AbstUI.Styles;
using AbstUI.Resources;
using AbstUI.SDL2.Resources;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.SDL2
{
    public static class AbstUISDLSetup
    {
        public static IServiceCollection WithAbstUISdl(this IServiceCollection services, Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
        {
            services
                .AddSingleton<IAbstSdlWindowManager, AbstSdlWindowManager>()
                .AddSingleton<IAbstFrameworkWindowManager>(p => p.GetRequiredService<IAbstSdlWindowManager>())
                .AddSingleton<AbstSdlComponentFactory>()
                .AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<AbstSdlComponentFactory>())
                //.AddTransient<IAbstFrameworkDialog, AbstSDLDialog>()
                .AddSingleton<IAbstFrameworkMainWindow, AbstSdlMainWindow>()


                .AddSingleton<IAbstFontManager, SdlFontManager>()
                .AddSingleton<IAbstStyleManager, AbstStyleManager>()
                .AddSingleton<IAbstResourceManager, SdlResourceManager>()
                .AddTransient(p => (SdlFontManager)p.GetRequiredService<IAbstFontManager>())
                .AddTransient(p => (AbstStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .AddSingleton<SdlFocusManager>()


                .AddSingleton<IAbstGlobalMouse, GlobalSDLAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalSDLAbstKey>()

                .WithAbstUI(componentRegistrations);

            return services;
        }
        private static bool _registerd = false;
        public static IServiceProvider WithAbstUISdl(this IServiceProvider services)
        {
            if (_registerd) return services; // only register once
            _registerd = true;
            services.WithAbstUI(); // need to be first to register all the windows in the windows factory.
            // We need to resolve once the SDL window manager to init
            services.GetRequiredService<IAbstSdlWindowManager>();

            //services.GetRequiredService<IAbstComponentFactory>()
            //     .Register<IAbstDialog,AbstGodotDialog>()
            //    ;
            return services;
        }
    }
}
