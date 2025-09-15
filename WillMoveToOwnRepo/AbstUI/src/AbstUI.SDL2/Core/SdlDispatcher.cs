using AbstUI.SDL2.SDLL;
using System.Collections.Concurrent;
using static AbstUI.SDL2.SDLL.SDL;

namespace AbstUI.SDL2.Core
{
   

    public class SdlDispatcher
    {
        private readonly ConcurrentQueue<Action> _queue = new();
        private readonly SDL_EventType _userEventType;

        public SdlDispatcher()
        {
            _userEventType = (SDL_EventType)SDL.SDL_RegisterEvents(1);
        }

        public void Post(Action action)
        {
            _queue.Enqueue(action);
            var ev = new SDL.SDL_Event
            {
                type = _userEventType
            };
            SDL.SDL_PushEvent(ref ev);
        }

        public void Pump(SDL.SDL_Event e)
        {
            if (e.type == _userEventType)
            {
                while (_queue.TryDequeue(out var action))
                    action();
            }
        }
    }

}
