using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Tempos;
using System.Security.Cryptography;
using LingoEngine.Members;

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

public class LingoColorPaletteSprite : LingoSprite, ILingoSpriteWithMember
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
    public override Action<LingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<LingoSprite> action = s => { };
        var settings = GetSettings();
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (LingoColorPaletteSprite)s;
            sprite.Member = Member;
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }

    public void SetMember(LingoColorPaletteMember member)
    {
        Member = member;
        SetSettings(member.GetSettings());
    }

    public ILingoMember? GetMember() => Member;
}
