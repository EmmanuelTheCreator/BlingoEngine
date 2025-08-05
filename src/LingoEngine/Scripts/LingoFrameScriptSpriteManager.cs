using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Scripts
{
    
    public interface ILingoFrameScriptSpriteManager : ILingoSpriteManager<LingoFrameScriptSprite>
    {
        //LingoSpriteFrameScript Add(int frame);
        LingoFrameScriptSprite Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoFrameScriptSprite>? configure = null) where TBehaviour : LingoSpriteBehavior;
        LingoFrameScriptSprite Add(int frameNumber, Action<LingoFrameScriptSprite>? configure = null);
    }
    internal class LingoFrameScriptSpriteManager : LingoSpriteManager<LingoFrameScriptSprite>, ILingoFrameScriptSpriteManager
    {
        
        public LingoFrameScriptSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoFrameScriptSprite.SpriteNumOffset, movie, environment)
        {
        }

        //public LingoSpriteFrameScript Add(int frame) => AddSprite(6, "FrameScript_" + frame, sprite => sprite.BeginFrame = frame);

        protected override LingoFrameScriptSprite OnCreateSprite(LingoMovie movie, Action<LingoFrameScriptSprite> onRemove) => new LingoFrameScriptSprite(_environment, onRemove);

        public LingoFrameScriptSprite Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoFrameScriptSprite>? configure = null) where TBehaviour : LingoSpriteBehavior
        {
            var sprite = Add(frameNumber, configure);
            var behaviour = sprite.SetBehavior<TBehaviour>();
            configureBehaviour?.Invoke(behaviour);
            return sprite;
        }
        public LingoFrameScriptSprite Add(int frameNumber, Action<LingoFrameScriptSprite>? configure = null) 
        {
            var sprite = AddSprite(1, $"FrameSprite_{frameNumber}", configure);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;
            configure?.Invoke(sprite);
            return sprite;
        }


        //internal void MoveFrameBehavior(int previousFrame, int newFrame)
        //{
        //    if (previousFrame == newFrame) return;
        //    if (!_frameSpriteBehaviors.TryGetValue(previousFrame, out var sprite))
        //        return;

        //    _frameSpriteBehaviors.Remove(previousFrame);

        //    if (_frameSpriteBehaviors.TryGetValue(newFrame, out var existing))
        //        existing.RemoveMe();

        //    _frameSpriteBehaviors[newFrame] = sprite;
        //    sprite.BeginFrame = newFrame;
        //    sprite.EndFrame = newFrame;

        //    RaiseSpriteListChanged();
        //}
    }

}
