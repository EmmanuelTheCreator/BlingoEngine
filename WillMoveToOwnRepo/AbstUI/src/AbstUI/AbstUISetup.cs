using AbstUI.Inputs;
using AbstUI.Tools;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI
{
    public static class AbstUISetup
    {
        public static IServiceCollection WithAbstUI(this IServiceCollection services)
        {

            services
                .AddTransient<IAbstDialog, AbstDialog>()
                 .AddSingleton<IAbstShortCutManager, AbstShortCutManager>()
                 .AddSingleton<IAbstWindowManager, AbstWindowManager>()
            //       .AddSingleton<IAbstGlobalMouse, GlobalLingoMouse>()
            //       .AddSingleton<IGlobalAbstKey, AbstKey>()
                   ;
            return services;
        }
    }
}
