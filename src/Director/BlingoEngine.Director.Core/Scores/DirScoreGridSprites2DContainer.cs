using AbstUI.Components.Containers;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Scores.Sprites2D;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Scores
{
    public class DirScoreGridSprites2DContainer : DirScoreGridContainer
    {
        

        public DirScoreGridSprites2DContainer(IDirScoreManager scoreManager, Func<string, IAbstFrameworkPanel, IAbstWindowDialogReference?> showConfirmDialog) : base(scoreManager, 10, showConfirmDialog)
        {
        }
        public override void Dispose()
        {
            if (_movie != null)
                _movie.Sprite2DManager.SpriteListChanged -= SpriteListChanged;
            base.Dispose();
        }
        public override void SetMovie(BlingoMovie? movie)
        {
            if (_movie != null)
                _movie.Sprite2DManager.SpriteListChanged -= SpriteListChanged;
            base.SetMovie(movie);
            RecreateSpriteChannels();
            UpdateSize();
            UpdateChannelsPosition();
            if (_movie != null)
                _movie.Sprite2DManager.SpriteListChanged += SpriteListChanged;   
        }
        private void RecreateSpriteChannels()
        {
            if (_movie == null) return;
            var channels = new List<DirScoreSprite2DChannel>();
            for (int i = 0; i < _movie.MaxSpriteChannelCount; i++)
            {
                var spriteChannel = new DirScoreSprite2DChannel(i + BlingoSprite2D.SpriteNumOffset + 1, _scoreManager);
                channels.Add(spriteChannel);
                spriteChannel.SetMovie(_movie);
            }
            SetChannels(channels.ToArray());
        }

        private void SpriteListChanged(int spriteNumWithChannelNum)
        {
            if (_movie == null) return;
            if (_channels.Length == 0 || _channels.Length != _movie.MaxSpriteChannelCount)
                RecreateSpriteChannels();

            if (spriteNumWithChannelNum > _channels.Length)
                return;

            ((DirScoreSprite2DChannel)_channelsDic[spriteNumWithChannelNum]).SpriteListChanged(spriteNumWithChannelNum);
        }

    }
}

