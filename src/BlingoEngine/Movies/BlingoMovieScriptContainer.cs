
using BlingoEngine.Core;
using BlingoEngine.Events;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Movies
{
    internal class BlingoMovieScriptContainer
    {
        private readonly Dictionary<Type, IBlingoMovieScript> _movieScripts = new();
        private readonly IBlingoMovieEnvironment _movieEnvironment;
        private readonly BlingoEventMediator _eventMediator;

        public BlingoMovieScriptContainer(IBlingoMovieEnvironment movieEnvironment, BlingoEventMediator eventMediator)
        {
            _movieEnvironment = movieEnvironment;
            _eventMediator = eventMediator;
        }
        public void Add<T>() where T : BlingoMovieScript
        {
            var ms = ((BlingoMovie)_movieEnvironment.Movie).GetServiceProvider().GetRequiredService<T>();
            _movieScripts.Add(typeof(T), ms);
            _eventMediator.Subscribe(ms);
        }
        public void Remove(IBlingoMovieScript ms)
        {
            _movieScripts.Remove(ms.GetType());
            _eventMediator.Unsubscribe(ms);
        }

        public TResult? Call<T, TResult>(Func<T, TResult> action) where T : IBlingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                return action(script);
            return default;
        }
        public void Call<T>(Action<T> action) where T : IBlingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                action(script);
        }

        internal T? Get<T>() where T : IBlingoMovieScript
        {
            if (_movieScripts.TryGetValue(typeof(T), out var ms)) return (T)ms;
            return default;
        }

        internal void CallAll(Action<IBlingoMovieScript> actionOnAll)
        {
            foreach (var ms in _movieScripts.Values)  
                actionOnAll(ms); 
        }
    }
}

