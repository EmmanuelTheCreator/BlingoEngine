using LingoEngine.Movies;
using LingoEngine.Tempos;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.Core.Scores.Tempos;


internal partial class DirScoreTempoGridChannel : DirScoreChannel<ILingoTempoSpriteManager, DirScoreTempoSprite, LingoTempoSprite>
{
    //private readonly AcceptDialog _dialog = new();
    //private readonly HSlider _slider = new();
    private int _editFrame;
    public DirScoreTempoGridChannel(IDirScoreManager scoreManager)
        : base(LingoTempoSprite.SpriteNumOffset+1, scoreManager)
    {
        //_dialog.Title = "Tempo";
        //_slider.MinValue = 1;
        //_slider.MaxValue = 60;
        //_slider.Step = 1;
        //_dialog.AddChild(_slider);
        //_dialog.GetOkButton().Pressed += OnDialogOk;
        //AddChild(_dialog);
    }

    protected override DirScoreTempoSprite CreateUISprite(LingoTempoSprite sprite, IDirSpritesManager spritesManager) => new DirScoreTempoSprite(sprite, spritesManager);

    protected override ILingoTempoSpriteManager GetManager(LingoMovie movie) => movie.Tempos;



    protected override void OnDoubleClick(int frame, DirScoreTempoSprite? sprite)
    {
        if (sprite == null) return;
        _editFrame = frame;
        //_slider.Value = sprite.Sprite.Tempo > 0? sprite.Sprite.Tempo : _movie!.Tempo;
        //_dialog.PopupCentered(new Vector2I(200, 80));
    }

    private void OnDialogOk()
    {
        if (_manager == null) return;
        // _manager.Add(_editFrame, (int)_slider.Value);
        MarkDirty();
    }


}
