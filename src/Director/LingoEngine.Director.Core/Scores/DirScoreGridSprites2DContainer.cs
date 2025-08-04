using LingoEngine.Director.Core.Scores.Sprites2D;
using LingoEngine.Inputs;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Scores
{
    public class DirScoreGridSprites2DContainer : DirScoreGridContainer
    {
     

        public DirScoreGridSprites2DContainer(IDirScoreManager scoreManager, ILingoMouse mouse) : base(scoreManager, mouse, 10)
        {
        }
        public override void SetMovie(LingoMovie? movie)
        {
            SpriteListChanged();
            base.SetMovie(movie);
        }
        private void RecreateSpriteChannels()
        {
            if (_movie == null) return;
            var channels = new List<DirScoreSprite2DChannel>();
            var allSprites = _movie.Sprite2DManager.GetAllSprites();
            for (int i = 0; i < _movie.MaxSpriteChannelCount; i++)
            {
                var spriteChannel = new DirScoreSprite2DChannel(i + 7, _scoreManager);
                channels.Add(spriteChannel);
            }
            SetChannels(channels.ToArray());
        }

        internal void SpriteListChanged()
        {
            if (_movie == null) return;
            if (_channels.Length == 0)
                RecreateSpriteChannels();
            var allSprites = _movie.Sprite2DManager.GetAllSprites().GroupBy(x => x.SpriteNum);
            foreach (var spriteNumGroup in allSprites)
            {
                var channel = (DirScoreSprite2DChannel)_channelsDic[spriteNumGroup.Key];
                channel.SetSprites(spriteNumGroup);
             
            }
            _framework.RequireRedrawChannels(); 
        }



        

    }
}
