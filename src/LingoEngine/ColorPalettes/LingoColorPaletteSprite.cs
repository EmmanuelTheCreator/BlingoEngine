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
    public const int SpriteNumOffset = 1;

    private readonly Action<LingoColorPaletteSprite> _removeMe;

    public int Frame { get; set; }
  
    public LingoColorPaletteMember? Member { get; set; }
    override public int SpriteNumWithChannel => SpriteNumOffset + SpriteNum;

    public LingoColorPaletteSprite(ILingoMovieEnvironment environment, Action<LingoColorPaletteSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void OnRemoveMe()
    {
        _removeMe(this);
    }
    public void SetSettings(LingoColorPaletteFrameSettings settings)
    {
        if (Member == null)
            Member =_environment.CastLibsContainer.ActiveCast.Add<LingoColorPaletteMember>(0, "");
        Member.SetSettings(settings);
    }

    public LingoColorPaletteFrameSettings? GetSettings()
    {
        if (Member == null) return null;
        return Member.GetSettings();
    }
}
