using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.VerboseLanguage
{
    public abstract record BlingoVerboseBase
    {
        protected readonly BlingoPlayer _player;

        public BlingoVerboseBase(BlingoPlayer player)
        {
            _player = player;
        }
        protected void DoOnMember<TMember>(string memberName, string? castlibName, Action<TMember> action)
            where TMember : IBlingoMember
        {
            var member = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            if (member != null) action(member);
        }
        protected void DoOnMember<TMember>(string memberName, int castlib, Action<TMember> action)
             where TMember : IBlingoMember
        {
            var member = _player.CastLibs.GetMember<TMember>(memberName, castlib);
            if (member != null) action(member);
        }
        protected TValue DoOnMember<TMember,TValue>(string memberName, string? castlibName, Func<TMember, TValue> action)
            where TMember : IBlingoMember
        {
            var member = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            if (member != null)  return action(member);
            return default!;
        }
        protected TValue DoOnMember<TMember, TValue>(string memberName, int castlib, Func<TMember, TValue> action)
             where TMember : IBlingoMember
        {
            var member = _player.CastLibs.GetMember<TMember>(memberName, castlib);
            if (member != null) return action(member);
            return default!;
        }
        protected void DoOnSprite(int spritenum, Action<IBlingoSprite> action)
        {
            IBlingoSpriteChannel? spriteChannel = _player.ActiveMovie?.GetActiveSprite(spritenum);
            if (spriteChannel != null && spriteChannel.HasSprite()) action(spriteChannel);
        }
      
    }

}




