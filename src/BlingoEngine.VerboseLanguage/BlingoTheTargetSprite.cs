using BlingoEngine.Core;
using BlingoEngine.Sprites;

namespace BlingoEngine.VerboseLanguage
{
    public interface IBlingoVerboseTheOfSprite<T>
    {
        IBlingoTheTargetSprite<T> Of { get; }
    }

    public interface IBlingoTheTargetSprite<T>
    {
        IBlingoVerbosePropAccess<T> Sprite(int number);
    }

    public record BlingoTheTargetSprite<TValue> : BlingoTheTargetBase<IBlingoSprite, TValue>, IBlingoTheTargetSprite<TValue>, IBlingoVerboseTheOfSprite<TValue>
    {

        public BlingoTheTargetSprite(BlingoPlayer blingoPlayer, Func<IBlingoSprite, TValue> actionGet, Action<IBlingoSprite, TValue> actionSet)
             : base(blingoPlayer, actionGet, actionSet)
        {
        }

        public IBlingoTheTargetSprite<TValue> Of => this;
     

        public IBlingoVerbosePropAccess<TValue> Sprite(int number)
        {
            _element = _player.ActiveMovie?.GetActiveSprite(number);
            return this;
        }

    }
}






