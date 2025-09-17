using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Scripts
{

    /// <summary>
    /// Lingo Frame Script Sprite Manager interface.
    /// </summary>
    public interface IBlingoFrameScriptSpriteManager : IBlingoSpriteManager<BlingoFrameScriptSprite>
    {
        //BlingoSpriteFrameScript Add(int frame);
        BlingoFrameScriptSprite Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<BlingoFrameScriptSprite>? configure = null) where TBehaviour : BlingoSpriteBehavior;
        BlingoFrameScriptSprite Add(int frameNumber, Action<BlingoFrameScriptSprite>? configure = null);
        bool Has(int frameNumber);
        void Delete(int frameNumber);
    }
    internal class BlingoFrameScriptSpriteManager : BlingoSpriteManager<BlingoFrameScriptSprite>, IBlingoFrameScriptSpriteManager
    {

        public BlingoFrameScriptSpriteManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoFrameScriptSprite.SpriteNumOffset, movie, environment)
        {
        }

        //public BlingoSpriteFrameScript Add(int frame) => AddSprite(6, "FrameScript_" + frame, sprite => sprite.BeginFrame = frame);

        protected override BlingoFrameScriptSprite OnCreateSprite(BlingoMovie movie, Action<BlingoFrameScriptSprite> onRemove) => new BlingoFrameScriptSprite(_environment.Player, _environment.Factory, _environment.Events, onRemove);

        public BlingoFrameScriptSprite Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<BlingoFrameScriptSprite>? configure = null) where TBehaviour : BlingoSpriteBehavior
        {
            var sprite = Add(frameNumber, configure);
            var behaviour = sprite.SetBehavior<TBehaviour>();
            configureBehaviour?.Invoke(behaviour);
            return sprite;
        }
        public BlingoFrameScriptSprite Add(int frameNumber, Action<BlingoFrameScriptSprite>? configure = null)
        {
            var sprite = AddSprite(1, $"FrameSprite_{frameNumber}", configure);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;
            configure?.Invoke(sprite);
            return sprite;
        }
        protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
        {
            var sprite = Add(begin);
            if (member is BlingoMemberScript memberTyped)
                sprite.SetMember(memberTyped);
            return sprite;
        }

        public bool Has(int frameNumber) => _allTimeSprites.Any(s => s.BeginFrame == frameNumber);

        public void Delete(int frameNumber)
        {
            var sprite = _allTimeSprites.FirstOrDefault(s => s.BeginFrame == frameNumber);
            if (sprite != null)
                sprite.RemoveMe();
        }
    }

}

