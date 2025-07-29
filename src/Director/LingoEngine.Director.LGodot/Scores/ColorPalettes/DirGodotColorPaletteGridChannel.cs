using LingoEngine.Movies;
using LingoEngine.Director.LGodot.Scores.ColorPalettes;
using LingoEngine.ColorPalettes;
using Godot;
using LingoEngine.Director.Core.Sprites;

namespace LingoEngine.Director.LGodot.Scores;


internal partial class DirGodotColorPaletteGridChannel : DirGodotTopGridChannel<ILingoSpriteColorPaletteSpriteManager, DirGodotColorPaletteSprite, LingoColorPaletteSprite>
{
    private readonly AcceptDialog _dialog = new();
    private readonly HSlider _slider = new();
    private int _editFrame;
    private LingoColorPaletteFrameSettings? _currentSettings;
    public DirGodotColorPaletteGridChannel(IDirSpritesManager spritesManager)
        : base(2, spritesManager)
    {
        _dialog.Title = "Color palette";
        _slider.MinValue = 1;
        _slider.MaxValue = 30;
        _slider.Step = 1;
        _dialog.AddChild(_slider);
        _dialog.GetOkButton().Pressed += OnDialogOk;
        AddChild(_dialog);
    }

    protected override DirGodotColorPaletteSprite CreateUISprite(LingoColorPaletteSprite sprite, IDirSpritesManager spritesManager) => new DirGodotColorPaletteSprite(sprite, spritesManager);

    protected override ILingoSpriteColorPaletteSpriteManager GetManager(LingoMovie movie) => movie.ColorPalettes;


    protected override void OnDoubleClick(int frame, DirGodotColorPaletteSprite? sprite)
    {
        if (_manager == null) return;
        _editFrame = frame;
        _currentSettings = sprite != null?  sprite.GetSettings() : new LingoColorPaletteFrameSettings();
        _slider.Value = _currentSettings.Rate > 0 ? _currentSettings.Rate : 30;
        _dialog.PopupCentered(new Vector2I(200, 80));
    }

    private void OnDialogOk()
    {
        if (_manager == null || _currentSettings == null) return;
        _manager.Add(_editFrame, _currentSettings);
        MarkDirty();
    }
}
