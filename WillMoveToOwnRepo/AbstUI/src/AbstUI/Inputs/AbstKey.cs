namespace AbstUI.Inputs
{
    /// <summary>
    /// Used to monitor a user’s keyboard activity.
    /// Mirrors AbstUI's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface IAbstKey
    {
        /// <summary>
        /// Returns TRUE if the Command key is currently pressed (Mac only).
        /// </summary>
        bool CommandDown { get; }

        /// <summary>
        /// Returns TRUE if the Control key is currently pressed.
        /// </summary>
        bool ControlDown { get; }

        /// <summary>
        /// Returns the character corresponding to the most recently pressed key.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Returns the numeric key code for the last key pressed, useful for system-level key identification.
        /// </summary>
        int KeyCode { get; }

        /// <summary>
        /// Returns TRUE if the Option key (Mac) or Alt key (Windows) is currently pressed.
        /// </summary>
        bool OptionDown { get; }

        /// <summary>
        /// Returns TRUE if the Shift key is currently pressed.
        /// </summary>
        bool ShiftDown { get; }

        /// <summary>
        /// Returns TRUE if the specified character key is currently being pressed down.
        /// Equivalent to checking if a specific ASCII character is pressed.
        /// </summary>
        /// <param name="key">The character key to test (e.g., 'a', 'b', '1').</param>
        bool KeyPressed(char key);

        /// <summary>
        /// Returns TRUE if the specified special key is currently pressed.
        /// For example, BACKSPACE, ENTER, TAB, etc.
        /// </summary>
        /// <param name="key">A named key defined in the AbstKeyType enum.</param>
        bool KeyPressed(AbstUIKeyType key);
        bool KeyPressed(int keyCode);

        AbstKey Subscribe(IAbstKeyEventHandler<AbstKeyEvent> handler);
        AbstKey Unsubscribe(IAbstKeyEventHandler<AbstKeyEvent> handler);

        IAbstKey CreateNewInstance(IAbstActivationProvider provider);

    }

    /// <summary>
    /// Enumeration of special keys commonly referenced in AbstUI key events.
    /// </summary>
    public enum AbstUIKeyType
    {
        BACKSPACE,
        ENTER,
        QUOTE,
        RETURN,
        SPACE,
        TAB
    }


    /// <inheritdoc/>
    public class AbstKey : IAbstKey
    {
        private HashSet<IAbstKeyEventHandler<AbstKeyEvent>> _subscriptions = new();
        private IAbstFrameworkKey _frameworkObj;

        public AbstKey() { }
        public AbstKey(IAbstFrameworkKey frameworkObj)
        {
            _frameworkObj = frameworkObj;
        }
        protected void Init(IAbstFrameworkKey frameworkObj)
        {
            _frameworkObj = frameworkObj;
        }

        /// <summary>
        /// Creates a proxy key that emits events only while the <paramref name="provider"/> is activated.
        /// </summary>
        public IAbstKey CreateNewInstance(IAbstActivationProvider provider) => new ProxyKey(this, provider);

        public T Framework<T>() where T : IAbstFrameworkKey => (T)_frameworkObj;

        public bool ControlDown => _frameworkObj.ControlDown;
        public bool CommandDown => _frameworkObj.CommandDown;
        public bool OptionDown => _frameworkObj.OptionDown;
        public bool ShiftDown => _frameworkObj.ShiftDown;
        public bool KeyPressed(AbstUIKeyType key) => _frameworkObj.KeyPressed(key);
        public bool KeyPressed(char key) => _frameworkObj.KeyPressed(key);
        public bool KeyPressed(int keyCode) => _frameworkObj.KeyPressed(keyCode);
        public string Key => _frameworkObj.Key;
        public int KeyCode => _frameworkObj.KeyCode;

        public virtual void DoKeyUp()
        {
            var ev = new AbstKeyEvent(this, AbstKeyEventType.KeyUp);
            DoOnAll(x => x.RaiseKeyUp(ev));
        }
        public virtual void DoKeyDown()
        {
            var ev = new AbstKeyEvent(this, AbstKeyEventType.KeyDown);
            DoOnAll(x => x.RaiseKeyDown(ev));
        }
        protected virtual void DoOnAll(Action<IAbstKeyEventHandler<AbstKeyEvent>> action)
        {
            foreach (var subscription in _subscriptions)
                action(subscription);
        }
        /// <summary>
        /// Subscribe to key events.
        /// </summary>
        public AbstKey Subscribe(IAbstKeyEventHandler<AbstKeyEvent> handler)
        {
            if (_subscriptions.Contains(handler)) return this;
            _subscriptions.Add(handler);
            return this;
        }
        public AbstKey Unsubscribe(IAbstKeyEventHandler<AbstKeyEvent> handler)
        {
            _subscriptions.Remove(handler);
            return this;
        }
        private sealed class ProxyKey : AbstKey, IAbstKeyEventHandler<AbstKeyEvent>, IDisposable
        {
            private readonly AbstKey _parent;
            private readonly IAbstActivationProvider _provider;

            internal ProxyKey(AbstKey parent, IAbstActivationProvider provider)
                : base(parent.Framework<IAbstFrameworkKey>())
            {
                _parent = parent;
                _provider = provider;
                _parent.Subscribe(this);
            }

            public void RaiseKeyDown(AbstKeyEvent lingoKey)
            {
                if (!_provider.IsActivated) return;
                DoKeyDown();
            }

            public void RaiseKeyUp(AbstKeyEvent lingoKey)
            {
                if (!_provider.IsActivated) return;
                DoKeyUp();
            }

            public void Dispose()
            {
                _parent.Unsubscribe(this);
            }
        }
    }
}
