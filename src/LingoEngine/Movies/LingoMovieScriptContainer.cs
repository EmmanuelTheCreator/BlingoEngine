
using LingoEngine.Core;
using LingoEngine.Events;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Movies
{
    internal class LingoMovieScriptContainer
    {
        private readonly Dictionary<Type, ILingoMovieScript> _movieScripts = new();
        private readonly ILingoMovieEnvironment _movieEnvironment;
        private readonly LingoEventMediator _eventMediator;

        public LingoMovieScriptContainer(ILingoMovieEnvironment movieEnvironment, LingoEventMediator eventMediator)
        {
            _movieEnvironment = movieEnvironment;
            _eventMediator = eventMediator;
        }
        public void Add<T>() where T : LingoMovieScript
        {
            var ms = ((LingoMovie)_movieEnvironment.Movie).GetServiceProvider().GetRequiredService<T>();
            _movieScripts.Add(typeof(T), ms);
            _eventMediator.Subscribe(ms);
        }
        public void Remove(ILingoMovieScript ms)
        {
            _movieScripts.Remove(ms.GetType());
            _eventMediator.Unsubscribe(ms);
        }

        public TResult? Call<T, TResult>(Func<T, TResult> action) where T : ILingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                return action(script);
            return default;
        }
        public void Call<T>(Action<T> action) where T : ILingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                action(script);
        }

        internal T? Get<T>() where T : ILingoMovieScript
        {
            if (_movieScripts.TryGetValue(typeof(T), out var ms)) return (T)ms;
            return default;
        }

        internal void CallAll(Action<ILingoMovieScript> actionOnAll)
        {
            foreach (var ms in _movieScripts.Values)  
                actionOnAll(ms); 
        }
    }
}
