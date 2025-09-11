using LingoEngine.Core;
using LingoEngine.Members;

namespace LingoEngine.VerboseLanguage
{

    public interface ILingoVerboseTheOfMember<T>
    {
        ILingoTheTargetMember<T> Of { get; }
    }
    public interface ILingoTheTargetMember<T>
    {
        ILingoVerbosePropAccess<T> Member(string memberName, string? castlibName = null);
        ILingoVerbosePropAccess<T> Member(string memberName, int castlib);
        ILingoVerbosePropAccess<T> Member(int numberInCast, int castlib);
    }
    public record LingoTheTargetMember<TMember, TValue> : LingoTheTargetBase<TMember, TValue>, ILingoTheTargetMember<TValue>, ILingoVerboseTheOfMember<TValue>, ILingoVerbosePropAccess<TValue>
        where TMember : ILingoMember
    {

        public LingoTheTargetMember(LingoPlayer lingoPlayer, Func<TMember, TValue> actionGet, Action<TMember, TValue> actionSet)
            : base(lingoPlayer, actionGet, actionSet)
        {
           
        }


        public ILingoTheTargetMember<TValue> Of => this;
       

        public ILingoVerbosePropAccess<TValue> Member(string memberName, string? castlibName = null)
        {
            _element = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            return this;
        }

        public ILingoVerbosePropAccess<TValue> Member(string memberName, int castlib)
        {
            _element = _player.CastLib(castlib).GetMember<TMember>(memberName);
            return this;
        }

        public ILingoVerbosePropAccess<TValue> Member(int numberInCast, int castlib)
        {
            _element = _player.CastLib(castlib).GetMember<TMember>(numberInCast);
            return this;
        }


    }
}





