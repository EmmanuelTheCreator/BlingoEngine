using LingoEngine.Movies;
using LingoEngine.ColorPalettes;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.UI;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Primitives;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Director.Core.Scores.ColorPalettes;


internal partial class DirScoreColorPaletteGridChannel : DirScoreChannel<ILingoSpriteColorPaletteSpriteManager, DirScoreColorPaletteSprite, LingoColorPaletteSprite>
{
    private readonly ILingoColorPaletteDefinitions _paletteDefinitions;
    
    private LingoColorPaletteFrameSettings? _currentSettings;
    private IDirectorWindowDialogReference? _dialog;

    private KeyValuePair<string, string>[] _paletteOptions = new[] {
            new KeyValuePair<string, string>("PaletteTransition", "Palette Transition"),
            new KeyValuePair<string, string>("ColorCycling", "Color Cycling")
        };

    private KeyValuePair<string, string>[] _transitionOptions = new[] {
                    new KeyValuePair<string, string>("FadeToBlack", "Fade to Black"),
                    new KeyValuePair<string, string>("FadeToWhite", "Fade to White"),
                    new KeyValuePair<string, string>("DontFade", "Don't Fade")
                };
  

    public DirScoreColorPaletteGridChannel(IDirScoreManager scoreManager, ILingoColorPaletteDefinitions paletteDefinitions)
        : base(LingoColorPaletteSprite.SpriteNumOffset+1, scoreManager)
    {
        _paletteDefinitions = paletteDefinitions;
        IsSingleFrame = true;
    }

    protected override DirScoreColorPaletteSprite CreateUISprite(LingoColorPaletteSprite sprite, IDirSpritesManager spritesManager) => new DirScoreColorPaletteSprite(sprite, spritesManager);

    protected override ILingoSpriteColorPaletteSpriteManager GetManager(LingoMovie movie) => movie.ColorPalettes;

    internal override void ShowCreateSpriteDialog(int frameNumber, Action<LingoSprite?> newSprite)
    {
        var settings = new LingoColorPaletteFrameSettings();
        Action okAction = () =>
        {
            var sprite = _manager!.Add(frameNumber, settings);
            newSprite(sprite);
            _hasDirtySpriteList = true;
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }



    internal override void ShowSpriteDialog(LingoSprite sprite)
    {
        if (!(sprite is LingoColorPaletteSprite paletteSprite)) return;
        var settings = paletteSprite.GetSettings();
        if (settings == null)
            settings = new LingoColorPaletteFrameSettings();
        Action okAction = () =>
        {
            if (settings != null)
                paletteSprite.SetSettings(settings);
            MarkDirty();
        };
        ShowDialog(settings, okAction);
    }


    private void ShowDialog(LingoColorPaletteFrameSettings settings, Action okAction)
    {

        var panel = _scoreManager.Factory.CreatePanel("Panel Pallette Sprite");
        panel.Width = 510;
        panel.Height = 283;
        var xOffset = 150;
        var xMargin = 10;
        var labelWidth = 100;
        var allPalettes = _paletteDefinitions.GetAll().ToList();
        var paletteOptions = allPalettes
            .Select(p => new KeyValuePair<string, string>(p.Name, p.Name))
            .ToList();

        var selected = settings.ColorPaletteId >= 0 && settings.ColorPaletteId < allPalettes.Count() ? allPalettes[settings.ColorPaletteId] : allPalettes.First();

        AbstUIGfxCanvas colorsCanvas = _scoreManager.Factory.CreateGfxCanvas("Panel Pallette Canvas", 130, 130);
        colorsCanvas.X = 10;
        colorsCanvas.Y = 10;
        panel.AddItem(colorsCanvas);

        panel.SetLabelAt("PaletteLabel", xOffset, 10, "Palette:",11, labelWidth, AbstUI.Texts.AbstTextAlignment.Right);
        panel.SetComboBoxAt(paletteOptions, "Palette", xOffset + labelWidth + xMargin, 10, 120, selected.Name, key =>
        {
            var id = allPalettes.FindIndex(p => p.Name == key);
            settings.ColorPaletteId = id;
            selected = settings.ColorPaletteId >= 0 && settings.ColorPaletteId < allPalettes.Count() ? allPalettes[settings.ColorPaletteId] : allPalettes.First();
            RedrawColors(colorsCanvas, selected);
        });

        panel.SetLabelAt("ActionLabel", xOffset, 40, "Action:", 11, labelWidth, AbstUI.Texts.AbstTextAlignment.Right);
        panel.SetComboBoxAt(_paletteOptions, "Action", xOffset + labelWidth + xMargin, 40, 120, settings.Action.ToString(), key =>
        {
            if (Enum.TryParse<LingoColorPaletteAction>(key, out var action))
                settings.Action = action;
        });

        panel.SetLabelAt("RateLabel", xOffset, 70, "Rate FPS:", 11, labelWidth, AbstUI.Texts.AbstTextAlignment.Right);
        panel.SetInputNumberAt(settings, "Rate", xOffset + labelWidth + xMargin, 70, 100, s => s.Rate, 1, 30);

        panel.SetLabelAt("OptionsLabel", xOffset, 100, "Options:", 11, labelWidth, AbstUI.Texts.AbstTextAlignment.Right);
        panel.SetComboBoxAt(_transitionOptions, "TransitionOption", xOffset+ labelWidth + xMargin, 100, 120, settings.TransitionOption.ToString(), key =>
        {
            if (Enum.TryParse<LingoColorPaletteTransitionOption>(key, out var option))
                settings.TransitionOption = option;
        });
        RedrawColors(colorsCanvas, selected);

        panel.AddPopupButtons(okAction, CloseDialog);

        _dialog = _showConfirmDialog?.Invoke("Frame Properties: Palette", (IAbstUIFrameworkGfxPanel)panel.FrameworkObj);
    }
    private void RedrawColors(AbstUIGfxCanvas colorsCanvas, LingoColorPaletteDefinition definition)
    {
        colorsCanvas.Clear(AColors.White);
        colorsCanvas.DrawRect(new ARect(1, 1, colorsCanvas.Width-2, colorsCanvas.Height-2), AColors.White);
        colorsCanvas.DrawRect(new ARect(0, 0, colorsCanvas.Width, colorsCanvas.Height), AColor.FromHex("#bbbbbb"),false);
        var allColors = definition.Colors.ToList();
        var y = 0;
        for (int i = 0; i < allColors.Count; i++)
        {
            var color = allColors[i];
            var x = i % 16 * 8;
            y = i / 16 * 8;
            colorsCanvas.DrawRect(new ARect(x+1, y+1, x+8, y+8), color);
        }
    }

    private void CloseDialog()
    {
        if (_dialog == null) return;
        _dialog.Close();
        _dialog = null;
    }

}
