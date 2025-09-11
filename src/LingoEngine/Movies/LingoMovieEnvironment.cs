using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Projects;
using LingoEngine.Sounds;
using LingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LingoEngine.Transitions;

namespace LingoEngine.Movies
{
    /// <summary>
    /// Lingo Movie Environment interface.
    /// </summary>
    public interface ILingoMovieEnvironment
    {
        ILingoPlayer Player { get; }
        ILingoKey Key { get; }
        ILingoSound Sound { get; }
        ILingoStageMouse Mouse { get; }
        ILingoSystem System { get; }
        ILingoMovie Movie { get; }
        ILingoClock Clock { get; }
        ILingoFrameworkFactory Factory { get; }
        ILingoEventMediator Events { get; }
        ILogger Logger { get; }
        LingoGlobalVars Globals { get; }

        ILingoCast? GetCastLib(int number);
        ILingoCast? GetCastLib(string name);
        internal ILingoCastLibsContainer CastLibsContainer { get; }
        T? GetMember<T>(int number, int? castLibNum = null) where T : class, ILingoMember;
        T? GetMember<T>(string name, int? castLibNum = null) where T : class, ILingoMember;
    }
    public class LingoMovieEnvironment : ILingoMovieEnvironment, IDisposable
    {
        private LingoPlayer _player = null!;
        private LingoKey _key = null!;
        private LingoSound _sound = null!;
        private LingoStageMouse _mouse = null!;
        private LingoSystem _system = null!;
        private ILingoClock _clock = null!;
        private ILingoMovie _movie = null!;
        private LingoCastLibsContainer _castLibsContainer = null!;
        private LingoEventMediator _eventMediator = null!;
        private IServiceScope _scopedServiceProvider = null!;
        private ILingoServiceProvider _serviceProvider = null!;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoProjectSettings _projectSettings;
        private readonly Lazy<ILingoMemberFactory> _memberFactory;
        private readonly ILingoServiceProvider _rootServiceProvider;
        private readonly ILogger<LingoMovieEnvironment> _logger;
        private LingoGlobalVars _globals = null!;
        public ILingoEventMediator Events => _eventMediator;

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _key;

        public ILingoSound Sound => _sound;

        public ILingoStageMouse Mouse => _mouse;

        public ILingoSystem System => _system;

        public ILingoMovie Movie => _movie;

        public ILingoClock Clock => _clock;

        public ILingoFrameworkFactory Factory => _factory;
        public ILogger Logger => _logger;
        public LingoGlobalVars Globals => _globals;

#pragma warning disable CS8618
#pragma warning restore CS8618
        public LingoMovieEnvironment(ILingoServiceProvider rootServiceProvider, ILingoFrameworkFactory factory, LingoProjectSettings projectSettings, ILogger<LingoMovieEnvironment> logger)
        {
            _memberFactory = rootServiceProvider.GetRequiredService<Lazy<ILingoMemberFactory>>();
            _rootServiceProvider = rootServiceProvider;
            _factory = factory;
            _projectSettings = projectSettings;
            _logger = logger;
        }

        internal void Init(string name, int number, LingoPlayer player, LingoKey lingoKey, LingoSound sound, LingoStageMouse mouse, LingoStage stage, LingoSystem system, ILingoClock clock, LingoCastLibsContainer lingoCastLibsContainer, IServiceScope scopedServiceProvider, ILingoTransitionPlayer transitionPlayer, Action<LingoMovie> onRemoveMe, LingoGlobalVars globals)
        {
            _scopedServiceProvider = scopedServiceProvider;
            _serviceProvider = new LingoServiceProvider();
            _serviceProvider.SetServiceProvider(scopedServiceProvider.ServiceProvider);
            _eventMediator = (LingoEventMediator)_serviceProvider.GetRequiredService<ILingoEventMediator>();
            _player = player;
            _key = lingoKey;
            _sound = sound;
            _mouse = mouse;
            _system = system;
            _clock = clock;
            _globals = globals;
            _mouse.Subscribe(_eventMediator);
            _key.Subscribe(_eventMediator);
            _castLibsContainer = lingoCastLibsContainer;

            _movie = new LingoMovie(this, stage, transitionPlayer, _castLibsContainer, _memberFactory.Value, name, number, _eventMediator, m =>
            {
                onRemoveMe(m);
                Dispose();
            }, _projectSettings, _rootServiceProvider.GetRequiredService<ILingoFrameLabelManager>());
        }
        internal ILingoServiceProvider GetServiceProvider() => _serviceProvider;
        public void Dispose()
        {
            _mouse.Unsubscribe(_eventMediator);
            _scopedServiceProvider.Dispose();
        }

        public ILingoCastLibsContainer CastLibsContainer => _castLibsContainer;
        public ILingoCast? GetCastLib(int number) => _castLibsContainer[number];
        public ILingoCast? GetCastLib(string name) => _castLibsContainer[name];

        T? ILingoMovieEnvironment.GetMember<T>(int number, int? castLibNum) where T : class => _castLibsContainer.GetMember<T>(number, castLibNum);

        T? ILingoMovieEnvironment.GetMember<T>(string name, int? castLibNum) where T : class => _castLibsContainer.GetMember<T>(name, castLibNum);

        internal void SetMouse(LingoStageMouse newMouse)
        {
            _mouse.Unsubscribe(_eventMediator);
            _mouse = newMouse;
            ((LingoMovie)_movie).SetMouse(newMouse);
            _mouse.Subscribe(_eventMediator);
        }
    }
}
