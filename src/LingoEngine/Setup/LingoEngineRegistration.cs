using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using AbstUI.Commands;
using AbstUI.Primitives;
using AbstUI.Windowing;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Events;
using LingoEngine.Sprites;
using AbstUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Casts;
using AbstUI;

namespace LingoEngine.Setup
{
    public class LingoEngineRegistration : ILingoEngineRegistration
    {
        private readonly IServiceCollection _container;
        private readonly LingoProxyServiceCollection _proxy;
        private readonly Dictionary<string, MovieRegistration> _movies = new();
        private readonly List<(string Name, string FileName)> _fonts = new();
        private readonly List<Action<IServiceProvider>> _prebuildActions = new();
        private readonly List<Action<ILingoServiceProvider>> _buildActions = new();
        private Action<ILingoFrameworkFactory>? _frameworkFactorySetup;
        private IServiceProvider? _serviceProvider;
        private readonly ILingoServiceProvider _lingoServiceProvider;
        private Action<LingoProjectSettings> _projectSettingsSetup = p => { };
        private bool _hasBeenBuild = false;
        private LingoPlayer? _player;
        private Func<ILingoProjectFactory>? _makeFactoryMethod;
        private ILingoProjectFactory? _projectFactory;
        private ILingoMovie? _startupMovie;
        public ILingoServiceProvider ServiceProvider => _lingoServiceProvider;

        public LingoEngineRegistration(IServiceCollection container, ILingoServiceProvider lingoServiceProvider)
        {
            _container = container;
            _proxy = new LingoProxyServiceCollection(container);
            _lingoServiceProvider = lingoServiceProvider;
        }

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
            _movies.Clear();
            _fonts.Clear();
            _buildActions.Clear();

            if (_serviceProvider != null)
            {
                var cmdManager = _lingoServiceProvider.GetService<IAbstCommandManager>();
                cmdManager?.Clear(preserveNamespaceFragment);
                var eventMediator = _lingoServiceProvider.GetService<ILingoEventMediator>();
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
                _frameworkFactorySetup = f => setup((T)f);
            return this;
        }

        public ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup)
        {
            _projectSettingsSetup = setup;
            return this;
        }

        public ILingoEngineRegistration BuildDelayed()
        {
            if (_hasBeenBuild && _player != null) return this;
            CreateProjectFactory();
            return this;
        }

        public LingoPlayer Build()
        {
            if (_hasBeenBuild && _player != null) return _player;
            CreateProjectFactory();
            _serviceProvider = _container.BuildServiceProvider();
            return Build(_serviceProvider);
        }

        public LingoPlayer Build(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _lingoServiceProvider.SetServiceProvider(_serviceProvider);
            foreach (var preBuild in _prebuildActions)
                preBuild(serviceProvider);

            var player = _lingoServiceProvider.GetRequiredService<LingoPlayer>();
            player.SetActionOnNewMovie(ActionOnNewMovie);
            if (_frameworkFactorySetup != null)
                _frameworkFactorySetup(_lingoServiceProvider.GetRequiredService<ILingoFrameworkFactory>());
            _player = player;

            ApplyProjectSettings();
            InitializeProject();
            _hasBeenBuild = true;
            return player;
        }

        public ILingoProjectFactory BuildAndRunProject(Action<IServiceProvider>? afterStart = null)
        {
            Build();
            return RunProject(afterStart);
        }

        public ILingoProjectFactory RunProject(Action<IServiceProvider>? afterStart = null)
        {
            if (_projectFactory == null) throw new InvalidOperationException("Project factory has not been set up. Use AddProjectFactory<TLingoProjectFactory>() to set it up. and run Build first");
            if (_startupMovie != null)
                _projectFactory.Run(_startupMovie, !LingoEngineGlobal.IsRunningDirector);
            if (afterStart != null && _serviceProvider != null) afterStart(_serviceProvider);
            return _projectFactory;
        }

        public ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action)
        {
            var registration = new MovieRegistration(_proxy, name);
            action(registration);
            _movies.Add(name, registration);
            return this;
        }

        private void ActionOnNewMovie(LingoMovie movie)
        {
            var registration = _movies[movie.Name];
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
            _fonts.Add((name, pathAndName));
            return this;
        }

        public ILingoEngineRegistration AddBuildAction(Action<ILingoServiceProvider> buildAction)
        {
            _buildActions.Add(buildAction);
            return this;
        }
        public ILingoEngineRegistration AddPreBuildAction(Action<IServiceProvider> buildAction)
        {
            _prebuildActions.Add(buildAction);
            return this;
        }

        public ILingoEngineRegistration RegisterWindows(Action<IAbstFameworkWindowRegistrator>? windowRegistrations)
        {
            if (windowRegistrations == null) return this;
            AbstUISetup.WithAbstUI(windowRegistrations);
            return this;
        }
        public ILingoEngineRegistration RegisterComponents(Action<IAbstFameworkComponentRegistrator>? componentRegistrations)
        {
            if (componentRegistrations == null) return this;
            AbstUISetup.WithAbstUI(componentRegistrations);
            return this;
        }

        #region Projects

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
                CreateProjectFactory();
                _serviceProvider = _container.BuildServiceProvider();
                _lingoServiceProvider.SetServiceProvider(_serviceProvider);
                ApplyProjectSettings();
                InitializeProject();
            }

            return this;
        }

        private void CreateProjectFactory()
        {
            if (_makeFactoryMethod == null)
                return;
            _projectFactory = _makeFactoryMethod();
        }

        private void ApplyProjectSettings()
        {
            if (_serviceProvider == null || _player == null)
                return;

            var settings = _serviceProvider.GetRequiredService<LingoProjectSettings>();
            _projectSettingsSetup(settings);

            if (settings.StageWidth > 0 && settings.StageHeight > 0)
            {
                var stageWidth = Math.Min(settings.StageWidth, 800);
                var stageHeight = Math.Min(settings.StageHeight, 600);
                _player.Stage.Width = settings.StageWidth;
                _player.Stage.Height = settings.StageHeight;
                if (!LingoEngineGlobal.IsRunningDirector)
                {
                    var mainWindow = _serviceProvider.GetService<AbstMainWindow>();
                    mainWindow?.SetSize(new APoint(stageWidth, stageHeight));
                }
            }
        }

        private void InitializeProject()
        {
            if (_projectFactory == null || _serviceProvider == null || _player == null)
                return;
            LoadFonts(_lingoServiceProvider);
            _buildActions.ForEach(b => b(_lingoServiceProvider));
            _lingoServiceProvider.GetRequiredService<IAbstCommandManager>()
                .DiscoverAndSubscribe(_lingoServiceProvider);
            _projectFactory.LoadCastLibs(_lingoServiceProvider.GetRequiredService<LingoCastLibsContainer>(), _player);
            _startupMovie = _projectFactory.LoadStartupMovie(_lingoServiceProvider, _player);
        }

        private void LoadFonts(ILingoServiceProvider serviceProvider)
        {
            var fontsManager = serviceProvider.GetRequiredService<IAbstFontManager>();
            foreach (var font in _fonts)
                fontsManager.AddFont(font.Name, font.FileName);
            fontsManager.LoadAll();
        }

       
        #endregion


        private class MovieRegistration : IMovieRegistration
        {
            private readonly IServiceCollection _container;
            private readonly string _movieName;
            private readonly List<Action<LingoMovie>> _movieScripts = new();

            public MovieRegistration(IServiceCollection container, string movieName)
            {
                _container = container;
                _movieName = movieName;
            }

            public Action<LingoMovie>[] GetAllMovieScriptsCtors() => _movieScripts.ToArray();

            public IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior
            {
                _container.AddTransient<T>();
                return this;
            }

            public IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript
            {
                _container.AddScoped<T>();
                _movieScripts.Add(movie =>
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

