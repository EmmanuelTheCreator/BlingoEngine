namespace AbstUI.Inputs
{
    public enum AbstKeyEventType
    {
        KeyUp,
        KeyDown
    }

    public class AbstKeyEvent
    {
        public IAbstKey AbstUIKey { get; }
        public AbstKeyEventType Type { get; }
        public bool ContinuePropagation { get; set; } = true;

        public bool CommandDown => AbstUIKey.CommandDown;
        public bool ControlDown => AbstUIKey.ControlDown;
        public bool OptionDown => AbstUIKey.OptionDown;
        public bool ShiftDown => AbstUIKey.ShiftDown;
        public string Key => AbstUIKey.Key;
        public int KeyCode => AbstUIKey.KeyCode;

        public bool KeyPressed(AbstUIKeyType key) => AbstUIKey.KeyPressed(key);
        public bool KeyPressed(char key) => AbstUIKey.KeyPressed(key);
        public bool KeyPressed(int keyCode) => AbstUIKey.KeyPressed(keyCode);

        public AbstKeyEvent(IAbstKey key, AbstKeyEventType type)
        {
            AbstUIKey = key;
            Type = type;
        }
    }

    public interface IAbstKeyEventHandler<TAbstUIKeyEvent>
        where TAbstUIKeyEvent : AbstKeyEvent
    {
        void RaiseKeyDown(TAbstUIKeyEvent key);
        void RaiseKeyUp(TAbstUIKeyEvent key);
    }

    public interface IAbstKeyEventSubscription
    {
        void Release();
    }

    public class AbstKeyEventSubscription<TAbstUIKeyEvent> : IAbstKeyEventSubscription
        where TAbstUIKeyEvent : AbstKeyEvent
    {
        private readonly IAbstKeyEventHandler<TAbstUIKeyEvent> _target;
        private readonly Action<AbstKeyEventSubscription<TAbstUIKeyEvent>> _release;

        public AbstKeyEventSubscription(IAbstKeyEventHandler<TAbstUIKeyEvent> target, Action<AbstKeyEventSubscription<TAbstUIKeyEvent>> release)
        {
            _target = target;
            _release = release;
        }

        public void DoKeyDown(TAbstUIKeyEvent key) => _target.RaiseKeyDown(key);
        public void DoKeyUp(TAbstUIKeyEvent key) => _target.RaiseKeyUp(key);

        public void Release()
        {
            _release(this);
        }
    }
}

