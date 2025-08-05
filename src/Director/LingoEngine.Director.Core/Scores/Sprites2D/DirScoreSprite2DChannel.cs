using LingoEngine.Director.Core.Sprites;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Scores.Sprites2D
{
    internal class DirScoreSprite2DChannel : DirScoreChannel<LingoSprite2DManager, DirScoreSprite2D, LingoSprite2D>
    {
        private int _editFrame;
        public DirScoreSprite2DChannel(int channelNum, IDirScoreManager scoreManager)
            : base(channelNum, scoreManager, false)
        {
        }

        protected override DirScoreSprite2D CreateUISprite(LingoSprite2D sprite, IDirSpritesManager spritesManager) => new DirScoreSprite2D(sprite, spritesManager);

        protected override LingoSprite2DManager GetManager(LingoMovie movie) => movie.Sprite2DManager;



    

        private void OnDialogOk()
        {
            if (_manager == null) return;
            // _manager.Add(_editFrame, (int)_slider.Value);
            MarkDirty();
        }


    }
}
