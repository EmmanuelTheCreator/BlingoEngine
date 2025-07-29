namespace LingoEngine.ColorPalettes;

public class LingoColorPaletteFrameSettings
{
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
}