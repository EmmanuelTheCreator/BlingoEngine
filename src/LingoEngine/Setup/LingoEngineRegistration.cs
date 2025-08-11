using LingoEngine.Commands;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Styles;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using LingoEngine.Casts;
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

        public void UnloadMovie(string? preserveNamespaceFragment = null)
        {
            if (_player?.ActiveMovie is LingoMovie active)
            {
                if (active.IsPlaying)
                    active.Halt();
                _player.CloseMovie(active);
                _player.SetActiveMovie(null);
            }

            _player?.UnloadInternalCastLibs();

            _proxy.UnregisterMovie(preserveNamespaceFragment);
            _Movies.Clear();
            _Fonts.Clear();
            _BuildActions.Clear();

            if (_serviceProvider != null)
            {
                var cmdManager = _serviceProvider.GetService<ILingoCommandManager>();
                cmdManager?.Clear(preserveNamespaceFragment);
                var eventMediator = _serviceProvider.GetService<ILingoEventMediator>();
                eventMediator?.Clear(preserveNamespaceFragment);
            }
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
            return Build(_container.BuildServiceProvider());
        }

        public LingoPlayer Build(IServiceProvider serviceProvider)
        {
            if (_hasBeenBuild && _player != null) return _player;
            _serviceProvider = serviceProvider;
            var player = serviceProvider.GetRequiredService<LingoPlayer>();
            player.SetActionOnNewMovie(ActionOnNewMovie);
            if (_FrameworkFactorySetup != null)
                _FrameworkFactorySetup(serviceProvider.GetRequiredService<ILingoFrameworkFactory>());

            _player = player;
            InitializeProject();
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

        private void InitializeProject()
        {
            if (_makeFactoryMethod == null || _serviceProvider == null || _player == null)
                return;

            _projectFactory = _makeFactoryMethod();
            _projectSettingsSetup(_serviceProvider.GetRequiredService<LingoProjectSettings>());
            LoadFonts(_serviceProvider);
            _BuildActions.ForEach(b => b(_serviceProvider));
            _serviceProvider.GetRequiredService<ILingoCommandManager>()
                .DiscoverAndSubscribe(_serviceProvider);
            _projectFactory.LoadCastLibs(_serviceProvider.GetRequiredService<ILingoCastLibsContainer>(), _player);
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
                UnloadMovie();
            }

            _makeFactoryMethod = () =>
            {
                var factory = new TLingoProjectFactory();
                factory.Setup(this);
                return factory;
            };

            if (_hasBeenBuild && _serviceProvider != null)
            {
                InitializeProject();
            }

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

