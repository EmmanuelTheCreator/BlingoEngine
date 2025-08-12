using LingoEngine.Casts;
using LingoEngine.Events;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Tempos;
using System.Security.Cryptography;

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
    private readonly ILingoCast _activeCastlib;
    private readonly Action<LingoColorPaletteSprite> _removeMe;

    public int Frame { get; set; }
  
    public LingoColorPaletteMember? Member { get; set; }
    override public int SpriteNumWithChannel => SpriteNumOffset + SpriteNum;

    public LingoColorPaletteSprite(ILingoEventMediator mediator, ILingoCast activeCastlib, Action<LingoColorPaletteSprite> removeMe) : base(mediator)
    {
        _activeCastlib = activeCastlib;
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void OnRemoveMe()
    {
        Member?.ReleaseFromRefUser(this);
        _removeMe(this);
    }
    public void SetSettings(LingoColorPaletteFrameSettings settings)
    {
        if (Member == null)
        {
            Member = _activeCastlib.Add<LingoColorPaletteMember>(0, "");
            Member.UsedBy(this);
        }
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
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (LingoColorPaletteSprite)s;
            if (member != null)
                sprite.SetMember(member);
        };

        return action;
    }

    public void SetMember(LingoColorPaletteMember member)
    {
        Member?.ReleaseFromRefUser(this);
        Member = member;
        Member.UsedBy(this);
        SetSettings(member.GetSettings());
    }

    public void MemberHasBeenRemoved()
    {
        Member = null;
    }

    public ILingoMember? GetMember() => Member;
}
