using LingoEngine.Director.Core.Sprites;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Scores.Sprites2D
{
    internal class DirScoreSprite2DChannel : DirScoreChannel<LingoSprite2DManager, DirScoreSprite2D, LingoSprite2D>
    {
        private int _editFrame;
        public DirScoreSprite2DChannel(int channelNum, IDirScoreManager scoreManager)
            : base(channelNum, scoreManager)
        {
        }

        protected override DirScoreSprite2D CreateUISprite(LingoSprite2D sprite, IDirSpritesManager spritesManager) => new DirScoreSprite2D(sprite, spritesManager);

        protected override LingoSprite2DManager GetManager(LingoMovie movie) => movie.Sprite2DManager;



        protected override void OnDoubleClick(int frame, DirScoreSprite2D? sprite)
        {
            if (sprite == null) return;
            _editFrame = frame;
            //_slider.Value = sprite.Sprite.Tempo > 0? sprite.Sprite.Tempo : _movie!.Tempo;
            //_dialog.PopupCentered(new Vector2I(200, 80));
        }

        internal void SetSprites(IEnumerable<LingoSprite2D> sprites)
        {

        }

        private void OnDialogOk()
        {
            if (_manager == null) return;
            // _manager.Add(_editFrame, (int)_slider.Value);
            MarkDirty();
        }


    }
}
