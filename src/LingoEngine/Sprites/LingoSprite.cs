using LingoEngine.Events;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites.Events;
using System.Diagnostics;

namespace LingoEngine.Sprites
{
    [DebuggerDisplay("LingoSprite:{SpriteNum}) {Name}")]
    public abstract class LingoSprite : ILingoSpriteBase
    {
        //protected readonly ILingoMovieEnvironment _environment;
        protected readonly LingoEventMediator _eventMediator;
        private readonly List<object> _spriteActors = new();
        private bool _lock;
        private int _beginFrame;
        private int _endFrame;
        private string _name = string.Empty;
        protected ILingoFrameworkSprite _frameworkSprite;
        public ILingoFrameworkSprite FrameworkObj => _frameworkSprite;
        
        public int BeginFrame
        {
            get => _beginFrame;
            set
            {
                _beginFrame = value;
                NotifyAnimationChanged();
            }
        }
        public int EndFrame
        {
            get => _endFrame;
            set
            {
                _endFrame = value;
                NotifyAnimationChanged();
            }
        }

        public virtual string Name
        {
            get => _name; set
            {
                _name = value;
                NotifyAnimationChanged();
            }
        }
        /// <summary>
        /// This represents the puppetsprite controlled by script.
        /// </summary>
        public bool Puppet { get; set; }

        public int SpriteNum { get; protected set; }
        public abstract int SpriteNumWithChannel { get; }


        /// <summary>
        /// Whether this sprite is currently active (i.e., the playhead is within its frame span).
        /// </summary>
        public bool IsActive { get; internal set; }
        public bool IsSingleFrame { get; protected set; }
        public bool Lock
        {
            get => _lock;
            set
            {
                _lock = value;
                NotifyAnimationChanged();
            }
        }

        public bool IsDeleted { get; private set; }

        public LingoSpriteState? InitialState { get; set; }

        public event Action? AnimationChanged;

        public LingoSprite(ILingoEventMediator eventMediator)
        {
            _eventMediator = (LingoEventMediator)eventMediator;
        }
        internal void Init(int number, string name)
        {
            SpriteNum = number;
            Name = name;
        }



        #region Actors
        protected IEnumerable<T> GetActorsOfType<T>() => _spriteActors.OfType<T>();
        public void AddActor(IHasStepFrameEvent actor)
        {
            _spriteActors.Add(actor);
            if (IsActive)
            {
                _eventMediator.SubscribeStepFrame(actor, SpriteNum + 6);
                if (actor is IHasBeginSpriteEvent begin) begin.BeginSprite();
            }
        }

        protected void RemoveActor(IHasStepFrameEvent actor)
        {
            if (IsActive)
            {
                if (actor is IHasEndSpriteEvent end) end.EndSprite();
                _eventMediator.UnsubscribeStepFrame(actor);
            }
            _spriteActors.Remove(actor);
        }
        internal void CallActor<T>(Action<T> actionOnActor) where T : class
        {
            var actor = GetActorsOfType<T>().FirstOrDefault();
            if (actor == null) return;
            actionOnActor(actor);
        }

        internal TResult? CallActor<T, TResult>(Func<T, TResult> func) where T : class
        {
            var actor = GetActorsOfType<T>().FirstOrDefault();
            if (actor == null) return default;
            return func(actor);
        }
        #endregion



        internal virtual void DoBeginSprite()
        {
            if (InitialState != null)
                LoadState(InitialState);

            // Subscribe all actors
            foreach (var actor in _spriteActors)
            {
                _eventMediator.Subscribe(actor, SpriteNum + 6);
                if (actor is IHasBeginSpriteEvent begin) begin.BeginSprite();
            }

           

            BeginSprite();
        }
        protected virtual void BeginSprite() { }

        internal virtual void DoEndSprite()
        {

            foreach (var actor in _spriteActors)
            {
                if (actor is IHasEndSpriteEvent end) end.EndSprite();
                _eventMediator.Unsubscribe(actor);
            }
            EndSprite();
        }
        protected virtual void EndSprite() { }
        public virtual string GetFullName() => $"{SpriteNum}.{Name}";

        public void RemoveMe()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            OnRemoveMe();
            NotifyAnimationChanged();
        }
        public abstract void OnRemoveMe();

        protected void NotifyAnimationChanged()
        {
            AnimationChanged?.Invoke();
        }




        #region Clone/ Get/Load state
        public virtual Action<LingoSprite> GetCloneAction()
        {
            Action<LingoSprite> action = s => { };
            var isLocked = Lock;
            int channel = SpriteNum;
            int begin = BeginFrame;
            int end = EndFrame;
            string name = Name;

            action = s =>
            {
                s.Name = name;
                s.BeginFrame = begin;
                s.EndFrame = end;
                s.Lock = isLocked;
            };
            return action;
        }

        public void LoadState(LingoSpriteState state)
        {
            Name = state.Name;
            OnLoadState(state);
        }

        protected virtual void OnLoadState(LingoSpriteState state) { }

        public LingoSpriteState GetState()
        {
            var state = CreateState();
            state.Name = Name;
            OnGetState(state);
            return state;
        }

        protected virtual LingoSpriteState CreateState() => new();

        protected virtual void OnGetState(LingoSpriteState state) { }

        #endregion

    }
}
