using System;
using AbstUI.Primitives;
using LingoEngine.Casts;
using AbstUI.Commands;
using AbstUI.Resources;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Movies.Commands;
using LingoEngine.Sounds;
using LingoEngine.Stages;
using LingoEngine.Tools;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Transitions;
using LingoEngine.Transitions.TransitionLibrary;
using System.Threading.Tasks;


namespace LingoEngine.Core
{


    public class LingoPlayer : ILingoPlayer, IDisposable,
        IAbstCommandHandler<RewindMovieCommand>,
        IAbstCommandHandler<PlayMovieCommand>,
        IAbstCommandHandler<StepFrameCommand>
    {
        private readonly Lazy<CsvImporter> _csvImporter;
        private readonly LingoCastLibsContainer _castLibsContainer;
        private readonly LingoSound _sound;
        private readonly ILingoWindow _window;
        private readonly ILingoServiceProvider _serviceProvider;
        private readonly LingoGlobalVars _globals;
        private Action<LingoMovie> _actionOnNewMovie;
        private Dictionary<string, LingoMovieEnvironment> _moviesByName = new();
        private List<LingoMovieEnvironment> _movies = new();
        private List<CancellationTokenSource> _delayedActionsCts = new();

        private readonly LingoKey _lingoKey;
        private readonly LingoStage _stage;
        private readonly LingoSystem _system;
        private readonly LingoClock _clock;
        private LingoStageMouse _mouse;
        private SynchronizationContext? _uiContext;

        public ILingoFrameworkFactory Factory { get; private set; }
        public ILingoServiceProvider ServiceProvider => _serviceProvider;
        public ILingoClock Clock => _clock;
        public LingoKey Key => _lingoKey;
        public ILingoStage Stage => _stage;
        /// <inheritdoc/>
        public ILingoCastLibsContainer CastLibs => _castLibsContainer;
        public ILingoCast ActiveCastLib => _castLibsContainer.ActiveCast;

        /// <inheritdoc/>
        public ILingoSound Sound => _sound;
        public ILingoStageMouse Mouse => _mouse;


        /// <inheritdoc/>
        public int CurrentSpriteNum => 1;
        /// <inheritdoc/>
        public bool NetPreset => true;
        /// <inheritdoc/>
        public bool ActiveWindow => true;
        /// <inheritdoc/>
        public bool SafePlayer => throw new NotImplementedException();
        /// <inheritdoc/>
        public string OrganizationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationPath { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ProductName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public int LastClick => 1;
        /// <inheritdoc/>
        public int LastEvent => 1;
        /// <inheritdoc/>
        public int LastKey => 1;
        /// <inheritdoc/>
        public Version ProductVersion { get; set; } = new Version(1, 0, 0, 0);
        /// <inheritdoc/>
        public Func<string> AlertHook { get; set; } = () => "";
        /// <inheritdoc/>
        bool ILingoPlayer.SafePlayer { get; set; }
        public ILingoMovie? ActiveMovie { get; private set; }



        public event Action<ILingoMovie?>? ActiveMovieChanged;

        public LingoPlayer(ILingoServiceProvider serviceProvider, ILingoFrameworkFactory factory, ILingoCastLibsContainer castLibsContainer, ILingoWindow window, ILingoClock lingoClock, ILingoSystem lingoSystem, IAbstResourceManager resourceManager, LingoGlobalVars globals)
        {
            _csvImporter = new Lazy<CsvImporter>(() => new CsvImporter(resourceManager));
            _actionOnNewMovie = m => { };
            _serviceProvider = serviceProvider;
            Factory = factory;
            _castLibsContainer = (LingoCastLibsContainer)castLibsContainer;
            _sound = Factory.CreateSound(_castLibsContainer);
            _window = window;
            _clock = (LingoClock)lingoClock;
            _system = (LingoSystem)lingoSystem;
            _lingoKey = Factory.CreateKey();
            _stage = Factory.CreateStage(this);
            _mouse = Factory.CreateMouse(_stage);
            _uiContext = SynchronizationContext.Current;
            _globals = globals;
        }
        public void Dispose()
        {
            // todo
            foreach (var cts in _delayedActionsCts)
                cts.Cancel();
            _delayedActionsCts.Clear();
        }
        public void LoadStage(int width, int height, AColor backgroundColor)
        {
            Stage.Width = width;
            Stage.Height = height;
            Stage.BackgroundColor = backgroundColor;

        }

        /// <inheritdoc/>
        public void Alert(string message)
        {
            Console.WriteLine(message);
        }
        /// <inheritdoc/>
        public void AppMinimize()
        {

        }
        /// <inheritdoc/>
        public void Cursor(int cursorNum)
        {

        }
        /// <inheritdoc/>
        public void Halt()
        {

        }
        /// <inheritdoc/>
        public void Open(string applicationName)
        {

        }
        /// <inheritdoc/>
        public void Quit()
        {
        }
        /// <inheritdoc/>
        public bool WindowPresent()
        {
            return true;
        }
        public ILingoCast CastLib(int number) => _castLibsContainer[number];
        public ILingoCast CastLib(string name) => _castLibsContainer[name];
        public ILingoMovie NewMovie(string name, bool andActivate = true)
        {
            // Create the default cast
            if (_castLibsContainer.Count == 0)
                _castLibsContainer.AddCast("Internal");

            // Create a new movies scope, needed for behaviours.
            var scope = _serviceProvider.CreateScope();
            var transitionPlayer = new LingoTransitionPlayer(_stage, _clock, _serviceProvider.GetRequiredService<ILingoTransitionLibrary>());

            // Create the movie.
            var movieEnv = (LingoMovieEnvironment)scope.ServiceProvider.GetRequiredService<ILingoMovieEnvironment>();
            movieEnv.Init(name, _movies.Count + 1, this, _lingoKey, _sound, _mouse, _stage, _system, Clock, _castLibsContainer, scope, transitionPlayer, m =>
            {
                // On remove movie
                var movieEnvironment = m.GetEnvironment();
                _movies.Remove(movieEnvironment);
                _moviesByName.Remove(m.Name);
            }, _globals);
            var movieTyped = (LingoMovie)movieEnv.Movie;

            // Add him
            _movies.Add(movieEnv);
            _moviesByName.Add(name, movieEnv);

            Factory.AddMovie(_stage, movieTyped);

            // Add all movieScripts
            _actionOnNewMovie(movieTyped);

            // Activate him;
            if (andActivate)
            {
                SetActiveMovie(movieTyped);
            }
            return movieEnv.Movie;
        }
        public void CloseMovie(ILingoMovie movie)
        {
            var typed = (LingoMovie)movie;
            typed.RemoveMe();
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public ILingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv, bool isInternal = false)
        {
            return LoadCastLibFromCsvAsync(castlibName, pathAndFilenameToCsv, isInternal).GetAwaiter().GetResult();
        }

        public async Task<ILingoPlayer> LoadCastLibFromCsvAsync(string castlibName, string pathAndFilenameToCsv, bool isInternal = false)
        {
            var castLib = _castLibsContainer.AddCast(castlibName, isInternal);
            await _csvImporter.Value.ImportInCastFromCsvFileAsync(castLib, pathAndFilenameToCsv, true, x => Console.WriteLine("WARNING:" + x));
            return this;
        }

        public ILingoPlayer AddCastLib(string name, bool isInternal = false, Action<ILingoCast>? configure = null)
        {
            var castLib = _castLibsContainer.AddCast(name, isInternal);
            if (configure != null)
                configure(castLib);
            return this;
        }

        public void UnloadInternalCastLibs()
        {
            _castLibsContainer.RemoveInternal();
        }

        public void SetActiveMovie(LingoMovie? movie)
        {
            ActiveMovie = movie;
            _stage.SetActiveMovie(movie);
            ActiveMovieChanged?.Invoke(movie);
        }

        void ILingoPlayer.SetActiveMovie(ILingoMovie? movie) => SetActiveMovie(movie as LingoMovie);



        internal void SetActionOnNewMovie(Action<LingoMovie> actionOnNewMovie)
        {
            _actionOnNewMovie = actionOnNewMovie;
        }


        #region Commands
        public bool CanExecute(RewindMovieCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(RewindMovieCommand command)
        {
            if (ActiveMovie is LingoMovie movie)
                movie.GoTo(1);
            return true;
        }


        public bool CanExecute(StepFrameCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(StepFrameCommand command)
        {
            if (ActiveMovie is not LingoMovie movie) return true;
            int offset = command.Offset;
            if (movie.IsPlaying)
            {
                var steps = Math.Abs(offset);
                for (int i = 0; i < steps; i++)
                {
                    if (offset > 0) movie.NextFrame();
                    else movie.PrevFrame();
                }
            }
            else
            {
                var target = MathCompat.Clamp(movie.Frame + offset, 1, movie.FrameCount);
                movie.GoToAndStop(target);
            }
            return true;
        }

        public bool CanExecute(PlayMovieCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(PlayMovieCommand command)
        {
            if (ActiveMovie is not LingoMovie movie) return true;
            if (command.Frame.HasValue)
                movie.GoTo(command.Frame.Value);
            if (movie.IsPlaying)
                movie.Halt();
            else
                movie.Play();
            return true;
        }

        public bool CanExecute(SetFrameLabelCommand command) => ActiveMovie is LingoMovie;

        public ILingoEventMediator GetEventMediator() => _serviceProvider.GetRequiredService<ILingoEventMediator>();

        public void ReplaceMouseObj(LingoStageMouse newMouse)
        {
            _mouse = newMouse;
            foreach (var movie in _movies)
                movie.SetMouse(newMouse);
        }






        #endregion

        public void RunDelayed(Action action, int milliseconds, CancellationTokenSource? cts = null)
        {
            cts ??= new CancellationTokenSource();
            _delayedActionsCts.Add(cts);
            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    if (_uiContext != null)
                        _uiContext.Post(_ => action(), null);
                    else
                        action();
                }
                _delayedActionsCts.Remove(cts);
            }, TaskScheduler.Default);
        }
    }
}



