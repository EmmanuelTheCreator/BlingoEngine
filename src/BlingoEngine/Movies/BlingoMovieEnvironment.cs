using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Projects;
using BlingoEngine.Sounds;
using BlingoEngine.Stages;
using BlingoEngine.Transitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BlingoEngine.Movies
{
    /// <summary>
    /// Lingo Movie Environment interface.
    /// </summary>
    public interface IBlingoMovieEnvironment
    {
        IBlingoPlayer Player { get; }
        IBlingoKey Key { get; }
        IBlingoSound Sound { get; }
        IBlingoStageMouse Mouse { get; }
        IBlingoSystem System { get; }
        IBlingoMovie Movie { get; }
        IBlingoClock Clock { get; }
        IBlingoFrameworkFactory Factory { get; }
        IBlingoEventMediator Events { get; }
        ILogger Logger { get; }
        BlingoGlobalVars Globals { get; }

        IBlingoCast? GetCastLib(int number);
        IBlingoCast? GetCastLib(string name);
        internal IBlingoCastLibsContainer CastLibsContainer { get; }
        T? GetMember<T>(int number, int? castLibNum = null) where T : class, IBlingoMember;
        T? GetMember<T>(string name, int? castLibNum = null) where T : class, IBlingoMember;

        T GetRequiredService<T>() where T : notnull;
    }
    public class BlingoMovieEnvironment : IBlingoMovieEnvironment, IDisposable
    {
        private BlingoPlayer _player = null!;
        private BlingoKey _key = null!;
        private BlingoSound _sound = null!;
        private BlingoStageMouse _mouse = null!;
        private BlingoSystem _system = null!;
        private IBlingoClock _clock = null!;
        private IBlingoMovie _movie = null!;
        private BlingoCastLibsContainer _castLibsContainer = null!;
        private BlingoEventMediator _eventMediator = null!;
        private IServiceScope _scopedServiceProvider = null!;
        private IBlingoServiceProvider _serviceProvider = null!;
        private readonly IBlingoFrameworkFactory _factory;
        private readonly BlingoProjectSettings _projectSettings;
        private readonly Lazy<IBlingoMemberFactory> _memberFactory;
        private readonly IBlingoServiceProvider _rootServiceProvider;
        private readonly ILogger<BlingoMovieEnvironment> _logger;
        private BlingoGlobalVars _globals = null!;
        public IBlingoEventMediator Events => _eventMediator;

        public IBlingoPlayer Player => _player;

        public IBlingoKey Key => _key;

        public IBlingoSound Sound => _sound;

        public IBlingoStageMouse Mouse => _mouse;

        public IBlingoSystem System => _system;

        public IBlingoMovie Movie => _movie;

        public IBlingoClock Clock => _clock;

        public IBlingoFrameworkFactory Factory => _factory;
        public ILogger Logger => _logger;
        public BlingoGlobalVars Globals => _globals;

#pragma warning disable CS8618
#pragma warning restore CS8618
        public BlingoMovieEnvironment(IBlingoServiceProvider rootServiceProvider, IBlingoFrameworkFactory factory, BlingoProjectSettings projectSettings, ILogger<BlingoMovieEnvironment> logger)
        {
            _memberFactory = rootServiceProvider.GetRequiredService<Lazy<IBlingoMemberFactory>>();
            _rootServiceProvider = rootServiceProvider;
            _factory = factory;
            _projectSettings = projectSettings;
            _logger = logger;
        }

        internal void Init(string name, int number, BlingoPlayer player, BlingoKey blingoKey, BlingoSound sound, BlingoStageMouse mouse, BlingoStage stage, BlingoSystem system, IBlingoClock clock, BlingoCastLibsContainer blingoCastLibsContainer, IServiceScope scopedServiceProvider, IBlingoTransitionPlayer transitionPlayer, Action<BlingoMovie> onRemoveMe, BlingoGlobalVars globals)
        {
            _scopedServiceProvider = scopedServiceProvider;
            _serviceProvider = new BlingoServiceProvider();
            _serviceProvider.SetServiceProvider(scopedServiceProvider.ServiceProvider);
            _eventMediator = (BlingoEventMediator)_serviceProvider.GetRequiredService<IBlingoEventMediator>();
            _player = player;
            _key = blingoKey;
            _sound = sound;
            _mouse = mouse;
            _system = system;
            _clock = clock;
            _globals = globals;
            _mouse.Subscribe(_eventMediator);
            _key.Subscribe(_eventMediator);
            _castLibsContainer = blingoCastLibsContainer;

            _movie = new BlingoMovie(this, stage, transitionPlayer, _castLibsContainer, _memberFactory.Value, name, number, _eventMediator, m =>
            {
                onRemoveMe(m);
                Dispose();
            }, _projectSettings, _rootServiceProvider.GetRequiredService<IBlingoFrameLabelManager>());
        }
        internal IBlingoServiceProvider GetServiceProvider() => _serviceProvider;
        public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
        public void Dispose()
        {
            _mouse.Unsubscribe(_eventMediator);
            _scopedServiceProvider.Dispose();
        }

        public IBlingoCastLibsContainer CastLibsContainer => _castLibsContainer;
        public IBlingoCast? GetCastLib(int number) => _castLibsContainer[number];
        public IBlingoCast? GetCastLib(string name) => _castLibsContainer[name];

        T? IBlingoMovieEnvironment.GetMember<T>(int number, int? castLibNum) where T : class => _castLibsContainer.GetMember<T>(number, castLibNum);

        T? IBlingoMovieEnvironment.GetMember<T>(string name, int? castLibNum) where T : class => _castLibsContainer.GetMember<T>(name, castLibNum);

        internal void SetMouse(BlingoStageMouse newMouse)
        {
            _mouse.Unsubscribe(_eventMediator);
            _mouse = newMouse;
            ((BlingoMovie)_movie).SetMouse(newMouse);
            _mouse.Subscribe(_eventMediator);
        }
    }
}

