using LingoEngine.Core;

namespace LingoEngine.VerboseLanguage
{
    public class LingoVerboseMethods
    {
        protected readonly LingoPlayer _player;
        public ILingoVerbosePut Put(object? value) => new LingoVerbosePut(_player, value);
        public ILingoVerboseThe The => new LingoVerboseThe(_player);

        public T Get<T>(Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
        {
            var propAccess = actionOnThe(new LingoVerboseThe(_player));
            return propAccess.Value;
        }
        public void Set<T>(Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe, T value)
        {
            var propAccess = actionOnThe(new LingoVerboseThe(_player));
            propAccess.Value = value;
        }
        public ILingoVerbosePropAccess<T> Set<T>( Func<ILingoVerboseThe, ILingoVerbosePropAccess<T>> actionOnThe)
        {
            var propAccess = actionOnThe(new LingoVerboseThe(_player));
            return propAccess;
        }
       

        public LingoVerboseMethods(ILingoPlayer lingoPlayer)
        {
            _player = (LingoPlayer)lingoPlayer;
        }
    }

}
