using BlingoEngine.Inputs;

namespace BlingoEngine.Events
{
    //public interface IEventSubscription
    //{
    //    void Release();
    //}
    //public class BlingoEventMediator
    //{
    //    private List<Subscription> _subscriptions = new();

    //    public IEventSubscription Subscribe(Action action)
    //    {
    //        var subscription = new Subscription(() => action(), e => _subscriptions.Remove(e));
    //        _subscriptions.Add(subscription);
    //        return subscription;
    //    }
    //    public void Invoke()
    //    {
    //        _subscriptions.ForEach(s => s.Invoke());
    //    }
    //    private class Subscription : IEventSubscription
    //    {
    //        private readonly Action _doIt;
    //        private readonly Func<object, bool> _release;

    //        public Subscription(Action doIt, Func<object, bool> release)
    //        {
    //            _doIt = doIt;
    //            _release = release;
    //        }
    //        public void Invoke() => _doIt();

    //        public void Release()
    //        {
    //            _release(this);
    //        }
    //    }
    //}
    internal interface IBlingoMovieScriptListener : IBlingoMouseEventHandler
    {
        // Keyboard events
        void DoKeyDown(BlingoKey key);
        void DoKeyUp(BlingoKey key);

        // Mouse events
        void DoMouseDown(BlingoStageMouse mouse);
        void DoMouseUp(BlingoStageMouse mouse);
        void DoMouseMove(BlingoStageMouse mouse);
        void DoMouseWheel(BlingoStageMouse mouse);

        // Movie Script Frame events
        void DoEnterFrame();
        void DoExitFrame();
    }
    internal interface IBlingoMovieScriptSubscription
    {
        void Release();
    }
    internal class BlingoMovieScriptMediator
    {
        private List<Subscription> _subscriptions = new();

        internal IBlingoMovieScriptSubscription Subscribe(IBlingoMovieScriptListener objectWithGlobalEvents)
        {
            var subscription = new Subscription(objectWithGlobalEvents, e => _subscriptions.Remove(e));
            _subscriptions.Add(subscription);
            return subscription;
        }
        // Dispatch KeyDown event to all subscribers
        public void KeyDown(BlingoKey key) => _subscriptions.ForEach(s => s.KeyDown(key));

        // Dispatch KeyUp event to all subscribers
        public void KeyUp(BlingoKey key) => _subscriptions.ForEach(s => s.KeyUp(key));

        // Dispatch MouseDown event to all subscribers
        public void MouseDown(BlingoStageMouse mouse) => _subscriptions.ForEach(s => s.MouseDown(mouse));

        // Dispatch MouseUp event to all subscribers
        public void MouseUp(BlingoStageMouse mouse) => _subscriptions.ForEach(s => s.MouseUp(mouse));

        // Dispatch MouseMove event to all subscribers
        public void MouseMove(BlingoStageMouse mouse) => _subscriptions.ForEach(s => s.MouseMove(mouse));
        public void MouseWheel(BlingoStageMouse mouse) => _subscriptions.ForEach(s => s.MouseWheel(mouse));

        // Dispatch EnterFrame event to all subscribers
        public void EnterFrame() => _subscriptions.ForEach(s => s.EnterFrame());

        // Dispatch ExitFrame event to all subscribers
        public void ExitFrame() => _subscriptions.ForEach(s => s.ExitFrame());
        private class Subscription : IBlingoMovieScriptSubscription
        {
            private readonly IBlingoMovieScriptListener _target;
            private readonly Action<Subscription> _release;

            public Subscription(IBlingoMovieScriptListener _target, Action<Subscription> release)
            {
                this._target = _target;
                _release = release;
            }
            public void KeyDown(BlingoKey key) => _target.DoKeyDown(key);
            public void KeyUp(BlingoKey key) => _target.DoKeyUp(key);
            public void MouseDown(BlingoStageMouse mouse) => _target.DoMouseDown(mouse);
            public void MouseUp(BlingoStageMouse mouse) => _target.DoMouseUp(mouse);
            public void MouseMove(BlingoStageMouse mouse) => _target.DoMouseMove(mouse);
            public void MouseWheel(BlingoStageMouse mouse) => _target.DoMouseWheel(mouse);
            public void EnterFrame() => _target.DoEnterFrame();
            public void ExitFrame() => _target.DoExitFrame();

            public void Release()
            {
                _release(this);
            }
        }
    }
}

