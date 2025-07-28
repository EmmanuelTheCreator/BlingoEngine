using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Sprites
{
    public abstract class LingoSprite : ILingoSpriteBase
    {
        protected readonly ILingoMovieEnvironment _environment;
        protected readonly LingoEventMediator _eventMediator;
        private readonly List<object> _spriteActors = new();

        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }

        public virtual string Name { get; set; } = string.Empty;
        /// <summary>
        /// This represents the puppetsprite controlled by script.
        /// </summary>
        public bool Puppet { get; set; }

        public int SpriteNum { get; protected set; }
        /// <summary>
        /// Whether this sprite is currently active (i.e., the playhead is within its frame span).
        /// </summary>
        public bool IsActive { get; internal set; }

        public LingoSprite(ILingoMovieEnvironment environment)
        {
            _environment = environment;
            _eventMediator = (LingoEventMediator)_environment.Events;
        }
        internal void Init(int number, string name)
        {
            SpriteNum = number;
            Name = name;
        }



        #region Actors
        protected IEnumerable<T> GetActorsOfType<T>() => _spriteActors.OfType<T>();
        protected void AddActor(object actor)
        {
            _spriteActors.Add(actor);
            if (IsActive)
            {
                _eventMediator.Subscribe(actor, SpriteNum + 6);
                if (actor is IHasBeginSpriteEvent begin) begin.BeginSprite();
            }
        }

        protected void RemoveActor(object actor)
        {
            if (IsActive)
            {
                if (actor is IHasEndSpriteEvent end) end.EndSprite();
                _eventMediator.Unsubscribe(actor);
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

        public abstract void RemoveMe();
    }
}
