using LingoEngine.Core;
using LingoEngine.Sprites;

namespace LingoEngine.VerboseLanguage
{
    public interface ILingoVerboseTheOfSprite<T>
    {
        ILingoTheTargetSprite<T> Of { get; }
    }

    public interface ILingoTheTargetSprite<T>
    {
        ILingoVerbosePropAccess<T> Sprite(int number);
    }

    public record LingoTheTargetSprite<TValue> : LingoTheTargetBase<ILingoSprite, TValue>, ILingoTheTargetSprite<TValue>, ILingoVerboseTheOfSprite<TValue>
    {

        public LingoTheTargetSprite(LingoPlayer lingoPlayer, Func<ILingoSprite, TValue> actionGet, Action<ILingoSprite, TValue> actionSet)
             : base(lingoPlayer, actionGet, actionSet)
        {
        }

        public ILingoTheTargetSprite<TValue> Of => this;
     

        public ILingoVerbosePropAccess<TValue> Sprite(int number)
        {
            _element = _player.ActiveMovie?.GetActiveSprite(number);
            return this;
        }

    }
}





