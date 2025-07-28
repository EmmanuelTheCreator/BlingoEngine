using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.LGodot.Scores.ColorPalettes;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Tools;
using LingoEngine.FrameworkCommunication;
using LingoEngine.ColorPalettes;
using Godot;

namespace LingoEngine.Director.LGodot.Scores;


internal partial class DirGodotColorPaletteGridChannel : DirGodotTopGridChannel<ILingoSpriteColorPaletteManager, DirGodotColorPaletteSprite, LingoColorPaletteSprite>
{
    private readonly AcceptDialog _dialog = new();
    private readonly HSlider _slider = new();
    private int _editFrame;
    public DirGodotColorPaletteGridChannel(DirScoreGfxValues gfxValues, IDirectorEventMediator mediator, ILingoFrameworkFactory factory, ILingoCommandManager commandManager)
        : base(gfxValues, mediator, factory, commandManager)
    {
        _dialog.Title = "Color palette";
        _slider.MinValue = 1;
        _slider.MaxValue = 60;
        _slider.Step = 1;
        _dialog.AddChild(_slider);
        _dialog.GetOkButton().Pressed += OnDialogOk;
        AddChild(_dialog);
    }

    protected override DirGodotColorPaletteSprite CreateUISprite(LingoColorPaletteSprite sprite) => new DirGodotColorPaletteSprite();

    protected override ILingoSpriteColorPaletteManager GetManager(LingoMovie movie) => movie.ColorPalettes;


    protected override void OnDoubleClick(int frame, DirGodotColorPaletteSprite? sprite)
    {
        if (_manager == null) return;
        _editFrame = frame;
        //_slider.Value = sprite.Sprite.PaletteId > 0 ? sprite.Sprite.PaletteId 0;
        _dialog.PopupCentered(new Vector2I(200, 80));
    }

    private void OnDialogOk()
    {
        if (_manager == null) return;
        _manager.Add(_editFrame);
        MarkDirty();
    }
}
