using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Scores.Sprites2D
{
    internal class DirScoreSprite2DChannel : DirScoreChannel<BlingoSprite2DManager, DirScoreSprite2D, BlingoSprite2D>
    {
        private int _editFrame;
        public DirScoreSprite2DChannel(int channelNum, IDirScoreManager scoreManager)
            : base(channelNum, scoreManager, false)
        {
        }

        protected override DirScoreSprite2D CreateUISprite(BlingoSprite2D sprite, IDirSpritesManager spritesManager) => new DirScoreSprite2D(sprite, spritesManager);

        protected override BlingoSprite2DManager GetManager(BlingoMovie movie) => movie.Sprite2DManager;



    

        private void OnDialogOk()
        {
            if (_manager == null) return;
            // _manager.Add(_editFrame, (int)_slider.Value);
            MarkDirty();
        }


    }
}

