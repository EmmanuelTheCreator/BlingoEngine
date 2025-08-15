namespace AbstUI.Inputs
{
    /// <summary>
    /// Used to monitor a user’s keyboard activity.
    /// Mirrors AbstUI's _key object functionality for key state and input monitoring.
    /// Example: isCtrlDown = _key.controlDown
    /// </summary>
    public interface IAbstUIKey
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

        AbstUIKey Subscribe(IAbstUIKeyEventHandler handler);
        AbstUIKey Unsubscribe(IAbstUIKeyEventHandler handler);

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
    public class AbstUIKey : IAbstUIKey
    {
        private HashSet<IAbstUIKeyEventHandler> _subscriptions = new();
        private readonly IAbstUIFrameworkKey _frameworkObj;

        public AbstUIKey(IAbstUIFrameworkKey frameworkObj)
        {
            _frameworkObj = frameworkObj;
        }

        /// <summary>
        /// Creates a proxy key that emits events only while the <paramref name="provider"/> is activated.
        /// </summary>
        public AbstUIKey CreateNewInstance(IAbstUIActivationProvider provider) => new ProxyKey(this, provider);

        internal T Framework<T>() where T : IAbstUIFrameworkKey => (T)_frameworkObj;

        public bool ControlDown => _frameworkObj.ControlDown;
        public bool CommandDown => _frameworkObj.CommandDown;
        public bool OptionDown => _frameworkObj.OptionDown;
        public bool ShiftDown => _frameworkObj.ShiftDown;
        public bool KeyPressed(AbstUIKeyType key) => _frameworkObj.KeyPressed(key);
        public bool KeyPressed(char key) => _frameworkObj.KeyPressed(key);
        public bool KeyPressed(int keyCode) => _frameworkObj.KeyPressed(keyCode);
        public string Key => _frameworkObj.Key;
        public int KeyCode => _frameworkObj.KeyCode;

        public virtual void DoKeyUp() => DoOnAll(x => x.RaiseKeyUp(this));
        public virtual void DoKeyDown() => DoOnAll(x => x.RaiseKeyDown(this));
        protected virtual void DoOnAll(Action<IAbstUIKeyEventHandler> action)
        {
            foreach (var subscription in _subscriptions)
                action(subscription);
        }
        /// <summary>
        /// Subscribe to key events.
        /// </summary>
        public AbstUIKey Subscribe(IAbstUIKeyEventHandler handler)
        {
            if (_subscriptions.Contains(handler)) return this;
            _subscriptions.Add(handler);
            return this;
        }
        public AbstUIKey Unsubscribe(IAbstUIKeyEventHandler handler)
        {
            _subscriptions.Remove(handler);
            return this;
        }
        private sealed class ProxyKey : AbstUIKey, IAbstUIKeyEventHandler, IDisposable
        {
            private readonly AbstUIKey _parent;
            private readonly IAbstUIActivationProvider _provider;

            internal ProxyKey(AbstUIKey parent, IAbstUIActivationProvider provider)
                : base(parent.Framework<IAbstUIFrameworkKey>())
            {
                _parent = parent;
                _provider = provider;
                _parent.Subscribe(this);
            }

            public void RaiseKeyDown(AbstUIKey lingoKey)
            {
                if (!_provider.IsActivated) return;
                DoKeyDown();
            }

            public void RaiseKeyUp(AbstUIKey lingoKey)
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
