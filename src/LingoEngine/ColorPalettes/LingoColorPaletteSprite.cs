using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.ColorPalettes;

public enum LingoColorPaletteAction
{
    PaletteTransition,
    ColorCycling
}
public enum LingoColorPaletteTransitionOption
{
    FadeToBlack,
    FadeToWhite,
    DontFade
}
public enum LingoColorPaletteCycleOption
{
    AutoReverse,
    Loop,
}

public class LingoColorPaletteSprite : LingoSprite
{
    private readonly Action<LingoColorPaletteSprite> _removeMe;

    public int Frame { get; set; }
    public int ColorPaletteId { get; set; }
    /// <summary>
    /// Range between 1 to 30 FPS
    /// in FPS
    /// </summary>
    public int Rate { get; set; }
    /// <summary>
    /// When Action is Cycling, the number of cycles.
    /// </summary>
    public int Cycles { get; set; } = 10;
    public LingoColorPaletteAction Action { get; set; } = LingoColorPaletteAction.PaletteTransition;
    public LingoColorPaletteTransitionOption TransitionOption { get; set; } = LingoColorPaletteTransitionOption.DontFade;
    public LingoColorPaletteCycleOption CycleOption { get; set; } = LingoColorPaletteCycleOption.AutoReverse;


    public LingoColorPaletteSprite(ILingoMovieEnvironment environment, Action<LingoColorPaletteSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void RemoveMe()
    {
        _removeMe(this);
    }
    public void SetSettings(LingoColorPaletteFrameSettings settings)
    {
        ColorPaletteId = settings.ColorPaletteId;
        Rate = settings.Rate;
        Action = settings.Action;
        CycleOption = settings.CycleOption;
        TransitionOption = settings.TransitionOption;
    }

    public LingoColorPaletteFrameSettings GetSettings()
    {
        return new LingoColorPaletteFrameSettings
        {
            Action = Action,
            ColorPaletteId = ColorPaletteId,
            CycleOption = CycleOption,
            Cycles = Cycles,
            Rate = Rate,
            TransitionOption = TransitionOption,
        };
    }
}
