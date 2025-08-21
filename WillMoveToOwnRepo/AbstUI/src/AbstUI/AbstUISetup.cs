using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Tools;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI
{
    public static class AbstUISetup
    {
        private static AbstFameworkComponentRegistrator? _registrator;

        public static IServiceCollection WithAbstUI(this IServiceCollection services,Action<IAbstFameworkComponentWinRegistrator>? componentRegistrations = null)
        {
            services
                .AddSingleton<IAbstCommandManager, AbstCommandManager>()
                .AddSingleton<IHistoryManager, HistoryManager>()
                .AddTransient<IAbstDialog, AbstDialog>()
                .AddTransient<AbstDialog>()
                .AddSingleton<AbstMainWindow>()
                .AddSingleton<IAbstShortCutManager, AbstShortCutManager>()
                .AddSingleton<IAbstWindowFactory, AbstWindowFactory>()
                .AddSingleton<AbstWindowManager>()
                .AddTransient(p => (IAbstWindowManager)p.GetRequiredService<AbstWindowManager>())
                .AddTransient(p => new Lazy<IServiceProvider>(() => p.GetRequiredService<IServiceProvider>()))
                .AddTransient(p => new Lazy<IAbstComponentFactory>(() => p.GetRequiredService<IAbstComponentFactory>()))
            //       .AddSingleton<IAbstGlobalMouse, GlobalLingoMouse>()
            //       .AddSingleton<IGlobalAbstKey, AbstKey>()
                   ;
            _registrator = new AbstFameworkComponentRegistrator(services);
            if (componentRegistrations != null)
                componentRegistrations(_registrator);
            return services;
        }
        public static void WithAbstUI(Action<IAbstFameworkComponentWinRegistrator> componentRegistrations)
        {
            if (_registrator == null) throw new InvalidOperationException("AbstUISetup has not been initialized. Call WithAbstUI first.");
            componentRegistrations(_registrator);
        }
        private static bool _registerd = false;
        public static IServiceProvider WithAbstUI(this IServiceProvider services)
        {
            if (_registerd) return services; // only register once
            _registerd = true;
            _registrator?.RegisterAll(services);
            return services;
        }
    }
}
