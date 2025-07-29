using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.LGodot.Scores.Tempos;
using LingoEngine.Tempos;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.LGodot.Scores;
internal partial class DirGodotTempoGridChannel : DirGodotTopGridChannel<ILingoSpriteTempoManager, DirGodotTempoSprite, LingoTempoSprite>
{
    private readonly AcceptDialog _dialog = new();
    private readonly HSlider _slider = new();
    private int _editFrame;
    public DirGodotTempoGridChannel(IDirSpritesManager spritesManager)
        : base(1, spritesManager)
    {
        _dialog.Title = "Tempo";
        _slider.MinValue = 1;
        _slider.MaxValue = 60;
        _slider.Step = 1;
        _dialog.AddChild(_slider);
        _dialog.GetOkButton().Pressed += OnDialogOk;
        AddChild(_dialog);
    }

    protected override DirGodotTempoSprite CreateUISprite(LingoTempoSprite sprite, IDirSpritesManager spritesManager) => new DirGodotTempoSprite(sprite, spritesManager);

    protected override ILingoSpriteTempoManager GetManager(LingoMovie movie) => movie.Tempos;

   

    protected override void OnDoubleClick(int frame, DirGodotTempoSprite? sprite)
    {
        if (sprite == null) return;
        _editFrame = frame;
        _slider.Value = sprite.Sprite.Tempo > 0? sprite.Sprite.Tempo : _movie!.Tempo;
        _dialog.PopupCentered(new Vector2I(200, 80));
    }

    private void OnDialogOk()
    {
        if (_manager == null) return;
        _manager.Add(_editFrame, (int)_slider.Value);
        MarkDirty();
    }

    
}
