using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Sprites
{
    public abstract class LingoSpriteBehavior : LingoScriptBase
    {
        protected LingoSprite2D Me;
        public LingoSprite2D GetSprite() => Me;

        /// <summary>
        /// Properties configured by the user via the property dialog.
        /// </summary>
        public BehaviorPropertiesContainer UserProperties { get; private set; } = new();
        public string Name { get; set; }

#pragma warning disable CS8618
        public LingoSpriteBehavior(ILingoMovieEnvironment env) : base(env)
#pragma warning restore CS8618
        {
            Name = GetType().Name;
        }

        internal void SetMe(LingoSprite2D sprite)
        {
            Me = sprite;
        }

        internal void SetUserProperties(BehaviorPropertiesContainer userProperties) => UserProperties = userProperties;
    }
}
