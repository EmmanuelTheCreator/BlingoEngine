using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.VerboseLanguage
{
    public abstract record LingoVerboseBase
    {
        protected readonly LingoPlayer _player;

        public LingoVerboseBase(LingoPlayer player)
        {
            _player = player;
        }
        protected void DoOnMember<TMember>(string memberName, string? castlibName, Action<TMember> action)
            where TMember : ILingoMember
        {
            var member = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            if (member != null) action(member);
        }
        protected void DoOnMember<TMember>(string memberName, int castlib, Action<TMember> action)
             where TMember : ILingoMember
        {
            var member = _player.CastLibs.GetMember<TMember>(memberName, castlib);
            if (member != null) action(member);
        }
        protected TValue DoOnMember<TMember,TValue>(string memberName, string? castlibName, Func<TMember, TValue> action)
            where TMember : ILingoMember
        {
            var member = !string.IsNullOrWhiteSpace(castlibName)
                ? _player.CastLib(castlibName!).GetMember<TMember>(memberName)
                : _player.CastLibs.GetMember<TMember>(memberName);
            if (member != null)  return action(member);
            return default!;
        }
        protected TValue DoOnMember<TMember, TValue>(string memberName, int castlib, Func<TMember, TValue> action)
             where TMember : ILingoMember
        {
            var member = _player.CastLibs.GetMember<TMember>(memberName, castlib);
            if (member != null) return action(member);
            return default!;
        }
        protected void DoOnSprite(int spritenum, Action<ILingoSprite> action)
        {
            ILingoSpriteChannel? spriteChannel = _player.ActiveMovie?.GetActiveSprite(spritenum);
            if (spriteChannel != null && spriteChannel.HasSprite()) action(spriteChannel);
        }
      
    }

}



