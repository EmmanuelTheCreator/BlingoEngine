using LingoEngine.Commands;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Sprites;
using LingoEngine.Styles;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
namespace LingoEngine.Setup
{
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration ServicesMain(Action<IServiceCollection> services);
        ILingoEngineRegistration ServicesLingo(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name, string pathAndName);
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup);
        LingoPlayer Build();
        ILingoProjectFactory BuildAndRunProject();
        LingoPlayer Build(IServiceProvider serviceProvider);
        ILingoEngineRegistration AddBuildAction(Action<IServiceProvider> buildAction);
        ILingoEngineRegistration SetProjectFactory<TLingoProjectFactory>() where TLingoProjectFactory : ILingoProjectFactory, new();
    }
    public interface IMovieRegistration
    {
        IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior;
        IMovieRegistration AddParentScript<T>() where T : LingoParentScript;
        IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript;
    }
    public static class LingoEngineRegistrationExtensions
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container, Action<ILingoEngineRegistration> config)
        {
            var engineRegistration = new LingoEngineRegistration(container);
            engineRegistration.RegisterCommonServices();
            container.AddSingleton<ILingoEngineRegistration>(engineRegistration);
            config(engineRegistration);
            return container;
        }
    }
    public class LingoEngineRegistration : ILingoEngineRegistration
    {
        private readonly IServiceCollection _container;
        private readonly LingoProxyServiceCollection _proxy;
        private readonly Dictionary<string, MovieRegistration> _Movies = new();
        private readonly List<(string Name, string FileName)> _Fonts = new();
        private readonly List<Action<IServiceProvider>> _BuildActions = new();
        private Action<ILingoFrameworkFactory>? _FrameworkFactorySetup;
        private IServiceProvider? _serviceProvider;
        private Action<LingoProjectSettings> _projectSettingsSetup = p => { };

        public LingoEngineRegistration(IServiceCollection container)
        {
            _container = container;
            _proxy = new LingoProxyServiceCollection(container);
        }

        public IServiceProvider? ServiceProvider => _serviceProvider;

        public void ClearDynamicRegistrations()
        {
            _proxy.UnregisterLingo();
            _Movies.Clear();
            _Fonts.Clear();
            _BuildActions.Clear();
        }
        public void RegisterCommonServices()
        {
            _container.WithGodotEngine();
        }

        public ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory
        {
            _container.AddSingleton<ILingoFrameworkFactory, T>();
            if (setup != null)
                _FrameworkFactorySetup = f => setup((T)f);
            return this;
        }

        public ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup)
        {
            _projectSettingsSetup = setup;
            return this;
        }


        private bool _hasBeenBuild = false;
        private LingoPlayer? _player;
        private Func<ILingoProjectFactory>? _makeFactoryMethod;
        private ILingoProjectFactory _projectFactory;

        public LingoPlayer Build()
        {
            if (_hasBeenBuild && _player != null) return _player;
            if (_makeFactoryMethod != null)
                _projectFactory = _makeFactoryMethod();
            return Build(_container.BuildServiceProvider());
        }

        public LingoPlayer Build(IServiceProvider serviceProvider)
        {
            if (_hasBeenBuild && _player != null) return _player;
            _serviceProvider = serviceProvider;
            _projectSettingsSetup(serviceProvider.GetRequiredService<LingoProjectSettings>());
            LoadFonts(serviceProvider);
            _BuildActions.ForEach(b => b(serviceProvider));
            var player = serviceProvider.GetRequiredService<LingoPlayer>();
            player.SetActionOnNewMovie(ActionOnNewMovie);
            if (_FrameworkFactorySetup != null)
                _FrameworkFactorySetup(serviceProvider.GetRequiredService<ILingoFrameworkFactory>());
            serviceProvider.GetRequiredService<ILingoCommandManager>()
                .DiscoverAndSubscribe(serviceProvider);

            _player = player;
            _hasBeenBuild = true;
            return player;
        }
        public ILingoProjectFactory BuildAndRunProject()
        {
            Build();
            return RunProject();
        }
        public ILingoProjectFactory RunProject()
        {
            if (_projectFactory == null) throw new InvalidOperationException("Project factory has not been set up. Use AddProjectFactory<TLingoProjectFactory>() to set it up. and run Build first");
            
            _projectFactory.Run(_serviceProvider!,_player!, !LingoEngineGlobal.IsRunningDirector);
            return _projectFactory;
        }

        private void LoadFonts(IServiceProvider serviceProvider)
        {
            var fontsManager = serviceProvider.GetRequiredService<ILingoFontManager>();
            foreach (var font in _Fonts)
                fontsManager.AddFont(font.Name, font.FileName);
            fontsManager.LoadAll();
        }

        public ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action)
        {
            var registration = new MovieRegistration(_proxy, name);
            action(registration);
            _Movies.Add(name, registration);
            return this;
        }
        private void ActionOnNewMovie(LingoMovie movie)
        {
            var registration = _Movies[movie.Name];
            var ctor = registration.GetAllMovieScriptsCtors();
            foreach (var item in ctor)
                item(movie);
        }

        public ILingoEngineRegistration ServicesMain(Action<IServiceCollection> services)
        {
            services(_container);
            return this;
        }

        public ILingoEngineRegistration ServicesLingo(Action<IServiceCollection> services)
        {
            services(_proxy);
            return this;
        }

        public ILingoEngineRegistration AddFont(string name, string pathAndName)
        {
            _Fonts.Add((name, pathAndName));
            return this;
        }

        public ILingoEngineRegistration AddBuildAction(Action<IServiceProvider> buildAction)
        {
            _BuildActions.Add(buildAction);
            return this;
        }

        public ILingoEngineRegistration SetTheProjectFactory(Type factoryType)
        {
            var method = typeof(ILingoEngineRegistration).GetMethod(nameof(SetProjectFactory), Type.EmptyTypes);
            var genericMethod = method!.MakeGenericMethod(factoryType);
            genericMethod.Invoke(this, null);
            return this;
        }

        public ILingoEngineRegistration SetProjectFactory<TLingoProjectFactory>() where TLingoProjectFactory : ILingoProjectFactory, new()
        {
            if (_makeFactoryMethod != null)
            {
                // there was already a project loaded, so unload the previous project
                // TODO : unload
            }
            _makeFactoryMethod = () =>
            {
                var factory = new TLingoProjectFactory();
                factory.Setup(this);
                return factory;
            };
            return this;
        }

        private class MovieRegistration : IMovieRegistration
        {
            private readonly IServiceCollection _container;
            private readonly string _movieName;
            private readonly List<Action<LingoMovie>> _MovieScripts = new();

            public MovieRegistration(IServiceCollection container, string movieName)
            {
                _container = container;
                _movieName = movieName;
            }
            public Action<LingoMovie>[] GetAllMovieScriptsCtors() => _MovieScripts.ToArray();

            public IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior
            {
                _container.AddTransient<T>();
                return this;
            }

            public IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript
            {
                _container.AddScoped<T>();
                _MovieScripts.Add(movie =>
                {
                    movie.AddMovieScript<T>();
                });
                return this;
            }
            public IMovieRegistration AddParentScript<T>() where T : LingoParentScript
            {
                _container.AddTransient<T>();
                return this;
            }


        }
    }
}

