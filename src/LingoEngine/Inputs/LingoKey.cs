using AbstUI.Inputs;
using LingoEngine.Events;
namespace LingoEngine.Inputs
{
    /// <summary>
    /// Used to monitor a user’s keyboard activity.
    /// Mirrors Lingo's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface ILingoKey : IAbstKey
    {
        LingoKey Subscribe(ILingoKeyEventHandler handler);

        LingoKey Unsubscribe(ILingoKeyEventHandler handler);
    }
    /// <summary>
    /// Lingo Framework Key interface.
    /// </summary>
    public interface ILingoFrameworkKey : IAbstFrameworkKey
    {

    }

    /// <inheritdoc/>
    public class LingoKey : AbstKey, ILingoKey
    {
        private HashSet<ILingoKeyEventHandler> _subscriptionsLingo = new();
        private readonly ILingoFrameworkKey _frameworkObjLingo;

        public LingoKey(ILingoFrameworkKey frameworkObj) : base(frameworkObj)
        {
            _frameworkObjLingo = frameworkObj;
        }

        ///// <summary>
        ///// Creates a proxy key that emits events only while the <paramref name="provider"/> is activated.
        ///// </summary>
        //public LingoKey CreateNewInstance(ILingoActivationProvider provider) => new ProxyKey(this, provider);

        //internal T Framework<T>() where T : ILingoFrameworkKey => (T)_frameworkObj;


        public override void DoKeyUp()
        {
            var ev = new LingoKeyEvent(this, AbstKeyEventType.KeyUp);
            base.DoOnAll(x => x.RaiseKeyUp(ev));
            DoOnAllLingo(x => x.RaiseKeyUp(ev));
        }

        public override void DoKeyDown()
        {
            var ev = new LingoKeyEvent(this, AbstKeyEventType.KeyDown);
            base.DoOnAll(x => x.RaiseKeyDown(ev));
            DoOnAllLingo(x => x.RaiseKeyDown(ev));
        }

        private void DoOnAllLingo(Action<ILingoKeyEventHandler> action)
        {
            foreach (var subscription in _subscriptionsLingo)
                action(subscription);
        }
        /// <summary>
        /// Subscribe to key events.
        /// </summary>
        public LingoKey Subscribe(ILingoKeyEventHandler handler)
        {
            if (_subscriptionsLingo.Contains(handler)) return this;
            _subscriptionsLingo.Add(handler);
            return this;
        }
        public LingoKey Unsubscribe(ILingoKeyEventHandler handler)
        {
            _subscriptionsLingo.Remove(handler);
            return this;
        }

    }
}
