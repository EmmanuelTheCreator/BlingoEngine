using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Scripts;
using BlingoEngine.Sprites;
using System;

namespace BlingoEngine.Sounds
{
    /// <summary>
    /// Lingo Sprite Audio Manager interface.
    /// </summary>
    public interface IBlingoSpriteAudioManager : IBlingoSpriteManager<BlingoSpriteSound>
    {


        BlingoSpriteSound Add(int channel, int startFrame, BlingoMemberSound sound);
    }
    internal class BlingoSpriteAudioManager : BlingoSpriteManager<BlingoSpriteSound>, IBlingoSpriteAudioManager
    {
        public BlingoSpriteAudioManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoSpriteSound.SpriteNumOffset, movie, environment)
        {
        }

        protected override BlingoSpriteSound OnCreateSprite(BlingoMovie movie, Action<BlingoSpriteSound> onRemove)
        {
            var clip = new BlingoSpriteSound(_environment.Sound, _environment.Events, onRemove);
            return clip;
        }


        public BlingoSpriteSound Add(int channel, int startFrame, BlingoMemberSound sound)
        {
            int lengthFrames = (int)Math.Ceiling(sound.Length * _movie.Tempo);
            int end = MathCompat.Clamp(startFrame + lengthFrames - 1, startFrame, _movie.FrameCount);
            return AddSprite(channel, "Audio " + channel, c =>
            {
                c.Init(channel, startFrame, end, sound);
            });
        }
        protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
        {
            if (!(member is BlingoMemberSound memberTyped)) return null;
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

