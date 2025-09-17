using AbstUI.Inputs;
using BlingoEngine.Events;
namespace BlingoEngine.Inputs
{
    /// <summary>
    /// Used to monitor a user's keyboard activity.
    /// Mirrors Lingo's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface IBlingoKey : IAbstKey
    {
        BlingoKey Subscribe(IBlingoKeyEventHandler handler);

        BlingoKey Unsubscribe(IBlingoKeyEventHandler handler);
    }
    /// <summary>
    /// Lingo Framework Key interface.
    /// </summary>
    public interface IBlingoFrameworkKey : IAbstFrameworkKey
    {

    }

    /// <inheritdoc/>
    public class BlingoKey : AbstKey, IBlingoKey
    {
        private HashSet<IBlingoKeyEventHandler> _subscriptionsBlingo = new();
        private readonly IBlingoFrameworkKey _frameworkObjBlingo;

        public BlingoKey(IBlingoFrameworkKey frameworkObj) : base(frameworkObj)
        {
            _frameworkObjBlingo = frameworkObj;
        }

        ///// <summary>
        ///// Creates a proxy key that emits events only while the <paramref name="provider"/> is activated.
        ///// </summary>
        //public BlingoKey CreateNewInstance(IBlingoActivationProvider provider) => new ProxyKey(this, provider);

        //internal T Framework<T>() where T : IBlingoFrameworkKey => (T)_frameworkObj;


        public override void DoKeyUp()
        {
            var ev = new BlingoKeyEvent(this, AbstKeyEventType.KeyUp);
            base.DoOnAll(x => x.RaiseKeyUp(ev));
            DoOnAllBlingo(x => x.RaiseKeyUp(ev));
        }

        public override void DoKeyDown()
        {
            var ev = new BlingoKeyEvent(this, AbstKeyEventType.KeyDown);
            base.DoOnAll(x => x.RaiseKeyDown(ev));
            DoOnAllBlingo(x => x.RaiseKeyDown(ev));
        }

        private void DoOnAllBlingo(Action<IBlingoKeyEventHandler> action)
        {
            foreach (var subscription in _subscriptionsBlingo)
                action(subscription);
        }
        /// <summary>
        /// Subscribe to key events.
        /// </summary>
        public BlingoKey Subscribe(IBlingoKeyEventHandler handler)
        {
            if (_subscriptionsBlingo.Contains(handler)) return this;
            _subscriptionsBlingo.Add(handler);
            return this;
        }
        public BlingoKey Unsubscribe(IBlingoKeyEventHandler handler)
        {
            _subscriptionsBlingo.Remove(handler);
            return this;
        }

    }
}

