using BlingoEngine.Core;

namespace BlingoEngine.VerboseLanguage
{
    public abstract record BlingoTheTargetBase<TElement, TValue> : IBlingoVerbosePropAccess<TValue>
    {
        private readonly Func<TElement, TValue> _actionGet;
        private readonly Action<TElement, TValue> _actionSet;

        protected readonly BlingoPlayer _player;
        protected TElement? _element;

        public BlingoTheTargetBase(BlingoPlayer blingoPlayer, Func<TElement, TValue> actionGet, Action<TElement, TValue> actionSet)
        {
            // we could use an expression tree but this is slower in performance.
            _player = blingoPlayer;
            _actionGet = actionGet;
            _actionSet = actionSet;
            _player = blingoPlayer;
        }

        public TValue this[IBlingoVerbosePropAccess<TValue> p]
        {
            get => p.Value;
            set => p.Value = value;
        }
        public TValue Value
        {
            get
            {
                if (_element == null) return default!;
                return _actionGet(_element);
            }
            set
            {
                if (_element == null) return;
                _actionSet(_element, value);
            }
        }

        public IBlingoVerbosePropAccess<TValue> To(TValue value)
        {
            Value = value;
            return this;
        }
    }
}






