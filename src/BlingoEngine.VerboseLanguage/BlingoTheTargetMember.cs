using BlingoEngine.Core;
using BlingoEngine.Members;

namespace BlingoEngine.VerboseLanguage
{

    public interface IBlingoVerboseTheOfMember<T>
    {
        IBlingoTheTargetMember<T> Of { get; }
    }
    public interface IBlingoTheTargetMember<T>
    {
        IBlingoVerbosePropAccess<T> Member(string memberName, string? castlibName = null);
        IBlingoVerbosePropAccess<T> Member(string memberName, int castlib);
        IBlingoVerbosePropAccess<T> Member(int numberInCast, int castlib);
    }
    public record BlingoTheTargetMember<TMember, TValue> : BlingoTheTargetBase<TMember, TValue>, IBlingoTheTargetMember<TValue>, IBlingoVerboseTheOfMember<TValue>, IBlingoVerbosePropAccess<TValue>
        where TMember : IBlingoMember
    {

        public BlingoTheTargetMember(BlingoPlayer blingoPlayer, Func<TMember, TValue> actionGet, Action<TMember, TValue> actionSet)
            : base(blingoPlayer, actionGet, actionSet)
        {
           
        }


        public IBlingoTheTargetMember<TValue> Of => this;
       

        public IBlingoVerbosePropAccess<TValue> Member(string memberName, string? castlibName = null)
        {
            _element = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            return this;
        }

        public IBlingoVerbosePropAccess<TValue> Member(string memberName, int castlib)
        {
            _element = _player.CastLib(castlib).GetMember<TMember>(memberName);
            return this;
        }

        public IBlingoVerbosePropAccess<TValue> Member(int numberInCast, int castlib)
        {
            _element = _player.CastLib(castlib).GetMember<TMember>(numberInCast);
            return this;
        }


    }
}






