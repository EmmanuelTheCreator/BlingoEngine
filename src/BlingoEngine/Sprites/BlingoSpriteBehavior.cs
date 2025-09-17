using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Scripts;

namespace BlingoEngine.Sprites
{
    public interface IBlingoSpriteBehavior : IBlingoScriptBase
    {
        BlingoSprite2D GetSprite();
        string Name { get; set; }
        BlingoMemberScript? ScriptMember { get; }
    }
    public abstract class BlingoSpriteBehavior : BlingoScriptBase, IBlingoSpriteBehavior
    {
        protected BlingoSprite2D Me;
        public BlingoSprite2D GetSprite() => Me;

        /// <summary>
        /// Properties configured by the user via the property dialog.
        /// </summary>
        public BehaviorPropertiesContainer UserProperties { get; private set; } = new();
        public string Name { get; set; }
        public BlingoMemberScript? ScriptMember { get; internal set; }

#pragma warning disable CS8618
        public BlingoSpriteBehavior(IBlingoMovieEnvironment env) : base(env)
#pragma warning restore CS8618
        {
            Name = GetType().Name;
        }

        internal void SetMe(BlingoSprite2D sprite)
        {
            Me = sprite;
        }

        internal void SetUserProperties(BehaviorPropertiesContainer userProperties) => UserProperties = userProperties;
    }
}

