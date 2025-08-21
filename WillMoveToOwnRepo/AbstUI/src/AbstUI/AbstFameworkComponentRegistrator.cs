using AbstUI.Components;
using AbstUI.FrameworkCommunication;
using AbstUI.Tools;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI
{
    public interface IAbstFameworkComponentWinRegistrator : IAbstFameworkWindowRegistrator, IAbstFameworkComponentRegistrator
    {

    }

    public interface IAbstFameworkWindowRegistrator
    {
        IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode) where TWindow : class, IAbstWindow;
        IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow;                          
        IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor) where TWindow : class, IAbstWindow;
        IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow;

        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode) where TWindow : class, IAbstWindow;
        
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow;
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor) where TWindow : class, IAbstWindow;
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow;

        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode)
            where TWindow : class, IAbstWindow
            where TImpl : class, IFrameworkFor<TWindow>;
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow where TImpl : class, IFrameworkFor<TWindow>;
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor) where TWindow : class, IAbstWindow where TImpl : class, IFrameworkFor<TWindow>;
        IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null) where TWindow : class, IAbstWindow where TImpl : class, IFrameworkFor<TWindow>;
    }
    public interface IAbstFameworkComponentRegistrator
    {
        public IAbstFameworkComponentRegistrator AddSingleton<TComponent, TFamework>()
         where TComponent : class
         where TFamework : class, IFrameworkFor<TComponent>;
        public IAbstFameworkComponentRegistrator AddTransient<TComponent, TFamework>()
         where TComponent : class
         where TFamework : class, IFrameworkFor<TComponent>;
    }
    public class AbstFameworkComponentRegistrator : IAbstFameworkComponentWinRegistrator
    {
        internal static AbstFameworkComponentRegistrator? I { get; private set; }
        private List<Action<IAbstWindowFactory, IAbstShortCutManager>> _windowregistrations = new();
        private List<Action<IAbstComponentFactory>> _registrations = new();
        private List<Action<IAbstComponentFactory>> _fwregistrations = new();
        private readonly IServiceCollection _serviceCollection;
        internal AbstFameworkComponentRegistrator(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            I = this;
        }


        #region Transients
        public IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode)
          where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddTransient<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddTransient<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, shortCutMap != null ? shortCutMap(s) : null));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddTransient<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, constructor));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddTransientWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddTransient<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, constructor, shortCutMap != null ? shortCutMap(s) : null));
            return this;
        }
        #endregion



        #region Singletons

        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode)
           where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddSingleton<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddSingleton<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, shortCutMap != null ? shortCutMap(s) : null));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddSingleton<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, constructor));
            return this;
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
            where TWindow : class, IAbstWindow
        {
            _serviceCollection.AddSingleton<TWindow>();
            _windowregistrations.Add((f, s) => f.Register<TWindow>(windowCode, constructor, shortCutMap != null ? shortCutMap(s) : null));
            return this;
        }


        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode)
             where TWindow : class, IAbstWindow
             where TImpl : class, IFrameworkFor<TWindow>
        {
            _serviceCollection.AddSingleton<TImpl>();
            _fwregistrations.Add(f => f.Register<TWindow, TImpl>());
            return AddSingletonWindow<TWindow>(windowCode);
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
           where TWindow : class, IAbstWindow
            where TImpl : class, IFrameworkFor<TWindow>
        {
            _serviceCollection.AddSingleton<TImpl>();
            _fwregistrations.Add(f => f.Register<TWindow, TImpl>());
            return AddSingletonWindow<TWindow>(windowCode);
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor)
            where TWindow : class, IAbstWindow
            where TImpl : class, IFrameworkFor<TWindow>
        {
            _serviceCollection.AddSingleton<TImpl>();
            _fwregistrations.Add(f => f.Register<TWindow, TImpl>());
            return AddSingletonWindow<TWindow>(windowCode);
        }
        public IAbstFameworkWindowRegistrator AddSingletonWindow<TWindow, TImpl>(string windowCode, Func<IServiceProvider, IAbstWindow> constructor, Func<IAbstShortCutManager, AbstShortCutMap>? shortCutMap = null)
            where TWindow : class, IAbstWindow
            where TImpl : class, IFrameworkFor<TWindow>
        {
            _serviceCollection.AddSingleton<TWindow>();
            _serviceCollection.AddSingleton<TImpl>();
            _fwregistrations.Add(f => f.Register<TWindow, TImpl>());
            return AddSingletonWindow<TWindow>(windowCode);
        }
        #endregion




        public IAbstFameworkComponentRegistrator AddTransient<TComponent, TFamework>() 
            where TComponent :class
            where TFamework : class, IFrameworkFor<TComponent>
        {
            _serviceCollection.AddTransient<TComponent>();
            _serviceCollection.AddTransient<TComponent>();
            _registrations.Add(f => f.Register<TComponent, TFamework>());
            return this;
        }
        public IAbstFameworkComponentRegistrator AddSingleton<TComponent, TFamework>() 
            where TComponent :class
            where TFamework : class,IFrameworkFor<TComponent>
        {
            _serviceCollection.AddSingleton<TComponent>();
            _serviceCollection.AddSingleton<TFamework>();
            _registrations.Add(f => f.Register<TComponent, TFamework>());
            return this;
        }
        internal void RegisterAll(IServiceProvider serviceProvider)
        {
            var windowFactory = serviceProvider.GetRequiredService<IAbstWindowFactory>();
            var shortCutManager = serviceProvider.GetRequiredService<IAbstShortCutManager>();
            foreach (var action in _windowregistrations)
                action(windowFactory, shortCutManager);
            
            var factory = serviceProvider.GetRequiredService<IAbstComponentFactory>();
            foreach (var registration in _registrations)
                registration(factory);
            foreach (var registration in _fwregistrations)
                registration(factory);
            
        }

     
    }
}
