using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Sounds
{
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
            var clip = new LingoSpriteSound(_environment,onRemove);
            return clip;
        }
   

        public LingoSpriteSound Add(int channel, int startFrame, LingoMemberSound sound)
        {
            int lengthFrames = (int)Math.Ceiling(sound.Length * _movie.Tempo);
            int end = Math.Clamp(startFrame + lengthFrames - 1, startFrame, _movie.FrameCount);
            return AddSprite(channel, "Audio "+channel, c =>
            {
                c.Init(channel, startFrame, end, sound);
            });
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
