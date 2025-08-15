using AbstUI.Inputs;
namespace LingoEngine.Inputs
{
    /// <summary>
    /// Used to monitor a user’s keyboard activity.
    /// Mirrors Lingo's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface ILingoKey : IAbstUIKey
    {
        LingoKey Subscribe(ILingoKeyEventHandler handler);

        LingoKey Unsubscribe(ILingoKeyEventHandler handler);
    }
    public interface ILingoFrameworkKey : IAbstUIFrameworkKey
    {

    }

    /// <inheritdoc/>
    public class LingoKey : AbstUIKey, ILingoKey
    {
        private HashSet<ILingoKeyEventHandler> _subscriptionsLingo = new();
        private readonly ILingoFrameworkKey _frameworkObjLingo;

        public LingoKey(ILingoFrameworkKey frameworkObj):base(frameworkObj)
        {
            _frameworkObjLingo = frameworkObj;
        }

        protected override void DoOnAll(Action<IAbstUIKeyEventHandler> action)
        {
            base.DoOnAll(action);
        }

        ///// <summary>
        ///// Creates a proxy key that emits events only while the <paramref name="provider"/> is activated.
        ///// </summary>
        //public LingoKey CreateNewInstance(ILingoActivationProvider provider) => new ProxyKey(this, provider);

        //internal T Framework<T>() where T : ILingoFrameworkKey => (T)_frameworkObj;


        public override void DoKeyUp()
        {
            base.DoOnAll(x => x.RaiseKeyUp(this));
            DoOnAllLingo(x => x.RaiseKeyUp(this));
        }

        public override void DoKeyDown()
        {
            base.DoOnAll(x => x.RaiseKeyDown(this));
            DoOnAllLingo(x => x.RaiseKeyDown(this));
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
