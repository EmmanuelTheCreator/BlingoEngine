using LingoEngine.Core;

namespace LingoEngine.VerboseLanguage
{
    public class LingoVerboseMethods
    {
        protected readonly LingoPlayer _player;
       
        public LingoVerboseMethods(ILingoPlayer lingoPlayer)
        {
            _player = (LingoPlayer)lingoPlayer;
        }
        public ILingoVerbosePut Put(object? value) => new LingoVerbosePut(_player, value);

        public ILingoVerboseThe The() => new LingoVerboseThe(_player);

        public ILingoVerbosePropAccess<T> Set<T>(ILingoVerbosePropAccess<T> thePropAccess) => thePropAccess;
        public LingoVerboseSetterProxy<T> Set<T>() => new LingoVerboseSetterProxy<T>();
        public T Get<T>(ILingoVerbosePropAccess<T> thePropAccess) => thePropAccess.Value;

        public bool Not(ILingoVerbosePropAccess<bool> currentValue) => !currentValue.Value;
    }
    public sealed class LingoVerboseSetterProxy<T>
    {
        public T this[ILingoVerbosePropAccess<T> p]
        {
            get => p.Value;
            set => p.Value = value;
        }
    }
}
