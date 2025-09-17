using BlingoEngine.Core;

namespace BlingoEngine.VerboseLanguage
{
    public class BlingoVerboseMethods
    {
        protected readonly BlingoPlayer _player;
       
        public BlingoVerboseMethods(IBlingoPlayer blingoPlayer)
        {
            _player = (BlingoPlayer)blingoPlayer;
        }
        public IBlingoVerbosePut Put(object? value) => new BlingoVerbosePut(_player, value);

        public IBlingoVerboseThe The() => new BlingoVerboseThe(_player);

        public IBlingoVerbosePropAccess<T> Set<T>(IBlingoVerbosePropAccess<T> thePropAccess) => thePropAccess;
        public BlingoVerboseSetterProxy<T> Set<T>() => new BlingoVerboseSetterProxy<T>();
        public T Get<T>(IBlingoVerbosePropAccess<T> thePropAccess) => thePropAccess.Value;

        public bool Not(IBlingoVerbosePropAccess<bool> currentValue) => !currentValue.Value;
    }
    public sealed class BlingoVerboseSetterProxy<T>
    {
        public T this[IBlingoVerbosePropAccess<T> p]
        {
            get => p.Value;
            set => p.Value = value;
        }
    }
}

