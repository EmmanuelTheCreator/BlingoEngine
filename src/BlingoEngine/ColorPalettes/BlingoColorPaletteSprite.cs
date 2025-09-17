using BlingoEngine.Casts;
using BlingoEngine.Events;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Tempos;
using System.Security.Cryptography;

namespace BlingoEngine.ColorPalettes;

public enum BlingoColorPaletteAction
{
    PaletteTransition,
    ColorCycling
}
public enum BlingoColorPaletteTransitionOption
{
    FadeToBlack,
    FadeToWhite,
    DontFade
}
public enum BlingoColorPaletteCycleOption
{
    AutoReverse,
    Loop,
}

public class BlingoColorPaletteSprite : BlingoSprite, IBlingoSpriteWithMember
{
    public const int SpriteNumOffset = 1;
    private readonly IBlingoCast _activeCastlib;
    private readonly Action<BlingoColorPaletteSprite> _removeMe;

    public int Frame { get; set; }
  
    public BlingoColorPaletteMember? Member { get; set; }
    override public int SpriteNumWithChannel => SpriteNumOffset + SpriteNum;



    public BlingoColorPaletteSprite(IBlingoEventMediator mediator, IBlingoCast activeCastlib, Action<BlingoColorPaletteSprite> removeMe) : base(mediator)
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
    public void SetSettings(BlingoColorPaletteFrameSettings settings)
    {
        if (Member == null)
        {
            Member = _activeCastlib.Add<BlingoColorPaletteMember>(0, "");
            Member.UsedBy(this);
        }
        Member.SetSettings(settings);
    }

    public BlingoColorPaletteFrameSettings? GetSettings()
    {
        if (Member == null) return null;
        return Member.GetSettings();
    }
    public override Action<BlingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<BlingoSprite> action = s => { };
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (BlingoColorPaletteSprite)s;
            if (member != null)
                sprite.SetMember(member);
        };

        return action;
    }

    public void SetMember(BlingoColorPaletteMember member)
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

    public IBlingoMember? GetMember() => Member;

    IBlingoMember? IBlingoSpriteWithMember.GetMember()
    {
        throw new NotImplementedException();
    }

    void IMemberRefUser.MemberHasBeenRemoved()
    {
        throw new NotImplementedException();
    }
}

