using LingoEngine.Core;

namespace LingoEngine.VerboseLanguage
{
    public abstract record LingoTheTargetBase<TElement, TValue> : ILingoVerbosePropAccess<TValue>
    {
        private readonly Func<TElement, TValue> _actionGet;
        private readonly Action<TElement, TValue> _actionSet;

        protected readonly LingoPlayer _player;
        protected TElement? _element;

        public LingoTheTargetBase(LingoPlayer lingoPlayer, Func<TElement, TValue> actionGet, Action<TElement, TValue> actionSet)
        {
            // we could use an expression tree but this is slower in performance.
            _player = lingoPlayer;
            _actionGet = actionGet;
            _actionSet = actionSet;
            _player = lingoPlayer;
        }

        public TValue this[ILingoVerbosePropAccess<TValue> p]
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

        public ILingoVerbosePropAccess<TValue> To(TValue value)
        {
            Value = value;
            return this;
        }
    }
}





