﻿using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Projects;
using LingoEngine.Sounds;
using LingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;

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

        ILingoCast? GetCastLib(int number);
        ILingoCast? GetCastLib(string name);
        internal ILingoCastLibsContainer CastLibsContainer { get; }
        T? GetMember<T>(int number, int? castLibNum = null) where T : class, ILingoMember;
        T? GetMember<T>(string name, int? castLibNum = null) where T : class, ILingoMember;
    }
    public class LingoMovieEnvironment : ILingoMovieEnvironment, IDisposable
    {
        private LingoPlayer _player;
        private LingoKey _key;
        private LingoSound _sound;
        private LingoStageMouse _mouse;
        private LingoSystem _system;
        private ILingoClock _clock;
        private ILingoMovie _movie;
        private LingoCastLibsContainer _castLibsContainer;
        private LingoEventMediator _eventMediator;
        private IServiceScope _scopedServiceProvider;
        private ILingoServiceProvider _serviceProvider;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoProjectSettings _projectSettings;
        private readonly Lazy<ILingoMemberFactory> _memberFactory;
        private readonly ILingoServiceProvider _rootServiceProvider;
        public ILingoEventMediator Events => _eventMediator;

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _key;

        public ILingoSound Sound => _sound;

        public ILingoStageMouse Mouse => _mouse;

        public ILingoSystem System => _system;

        public ILingoMovie Movie => _movie;

        public ILingoClock Clock => _clock;

        public ILingoFrameworkFactory Factory => _factory;

#pragma warning disable CS8618 
#pragma warning restore CS8618 
        public LingoMovieEnvironment(ILingoServiceProvider rootServiceProvider, ILingoFrameworkFactory factory, LingoProjectSettings projectSettings)
        {
            _memberFactory = rootServiceProvider.GetRequiredService<Lazy<ILingoMemberFactory>>();
            _rootServiceProvider = rootServiceProvider;
            _factory = factory;
            _projectSettings = projectSettings;
        }

        internal void Init(string name, int number, LingoPlayer player, LingoKey lingoKey, LingoSound sound, LingoStageMouse mouse, LingoStage stage, LingoSystem system, ILingoClock clock, LingoCastLibsContainer lingoCastLibsContainer, IServiceScope scopedServiceProvider, Action<LingoMovie> onRemoveMe)
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
            _mouse.Subscribe(_eventMediator);
            _key.Subscribe(_eventMediator);
            _castLibsContainer = lingoCastLibsContainer;
            _movie = new LingoMovie(this, stage, _castLibsContainer, _memberFactory.Value, name, number, _eventMediator, m =>
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

        T? ILingoMovieEnvironment.GetMember<T>(int number, int? castLibNum = null) where T : class => _castLibsContainer.GetMember<T>(number, castLibNum);

        T? ILingoMovieEnvironment.GetMember<T>(string name, int? castLibNum = null) where T : class => _castLibsContainer.GetMember<T>(name, castLibNum);

        internal void SetMouse(LingoStageMouse newMouse)
        {
            _mouse.Unsubscribe(_eventMediator);
            _mouse = newMouse;
            ((LingoMovie)_movie).SetMouse(newMouse);
            _mouse.Subscribe(_eventMediator);
        }
    }
}
