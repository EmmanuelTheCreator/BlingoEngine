using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Scripts
{
    
    public interface ILingoFrameScriptSpriteManager : ILingoSpriteManager<LingoSpriteFrameScript>
    {
        //LingoSpriteFrameScript Add(int frame);
        LingoSpriteFrameScript Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour, Action<LingoSpriteFrameScript>? configure) where TBehaviour : LingoSpriteBehavior;
    }
    internal class LingoFrameScriptSpriteManager : LingoSpriteManager<LingoSpriteFrameScript>, ILingoFrameScriptSpriteManager
    {
        
        public LingoFrameScriptSpriteManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoSpriteFrameScript.SpriteNumOffset, movie, environment)
        {
        }

        //public LingoSpriteFrameScript Add(int frame) => AddSprite(6, "FrameScript_" + frame, sprite => sprite.BeginFrame = frame);

        protected override LingoSpriteFrameScript OnCreateSprite(LingoMovie movie, Action<LingoSpriteFrameScript> onRemove) => new LingoSpriteFrameScript(_environment, onRemove);

        public LingoSpriteFrameScript Add<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour, Action<LingoSpriteFrameScript>? configure) where TBehaviour : LingoSpriteBehavior
        {
            var sprite = AddSprite(1, $"FrameSprite_{frameNumber}", configure);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;

            var behaviour = sprite.SetBehavior<TBehaviour>();
            configureBehaviour?.Invoke(behaviour);
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
