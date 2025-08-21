using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Scripts;
using LingoEngine.Sprites;
using System;

namespace LingoEngine.Sounds
{
    /// <summary>
    /// Lingo Sprite Audio Manager interface.
    /// </summary>
    public interface ILingoSpriteAudioManager : ILingoSpriteManager<LingoSpriteSound>
    {


        LingoSpriteSound Add(int channel, int startFrame, LingoMemberSound sound);
    }
    internal class LingoSpriteAudioManager : LingoSpriteManager<LingoSpriteSound>, ILingoSpriteAudioManager
    {
        public LingoSpriteAudioManager(LingoMovie movie, LingoMovieEnvironment environment) : base(LingoSpriteSound.SpriteNumOffset, movie, environment)
        {
        }

        protected override LingoSpriteSound OnCreateSprite(LingoMovie movie, Action<LingoSpriteSound> onRemove)
        {
            var clip = new LingoSpriteSound(_environment.Sound, _environment.Events, onRemove);
            return clip;
        }


        public LingoSpriteSound Add(int channel, int startFrame, LingoMemberSound sound)
        {
            int lengthFrames = (int)Math.Ceiling(sound.Length * _movie.Tempo);
#if NET48
            int end = MathCompat.Clamp(startFrame + lengthFrames - 1, startFrame, _movie.FrameCount);
#else
            int end = Math.Clamp(startFrame + lengthFrames - 1, startFrame, _movie.FrameCount);
#endif
            return AddSprite(channel, "Audio " + channel, c =>
            {
                c.Init(channel, startFrame, end, sound);
            });
        }
        protected override LingoSprite? OnAdd(int spriteNum, int begin, int end, ILingoMember? member)
        {
            if (!(member is LingoMemberSound memberTyped)) return null;
            var sprite = Add(spriteNum, begin, memberTyped);
            return sprite;
        }
        public override void MuteChannel(int channel, bool state)
        {
            base.MuteChannel(channel, state);
            var channelS = _environment.Sound.Channel(channel);
            if (channelS == null)
                return;
            channelS.Mute = state;

        }
    }
}
