using BlingoEngine.Sprites;
using BlingoEngine.Members;
using BlingoEngine.Events;
using BlingoEngine.Casts;

namespace BlingoEngine.Transitions;

public class BlingoTransitionSprite : BlingoSprite, IBlingoSpriteWithMember
{
    public const int SpriteNumOffset = 2;
    private readonly IBlingoCast _castlib;
    private readonly Action<BlingoTransitionSprite> _removeMe;

    public int Frame { get; set; }
    public int TransitionId { get; set; }

    public BlingoTransitionMember? Member { get; set; }
    public override int SpriteNumWithChannel => SpriteNumOffset + SpriteNum;

    public BlingoTransitionSprite(IBlingoEventMediator mediator, IBlingoCast castlib, Action<BlingoTransitionSprite> removeMe) : base(mediator)
    {
        _castlib = castlib;
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void OnRemoveMe()
    {
        Member?.ReleaseFromRefUser(this);
        _removeMe(this);
    }
    public void SetSettings(BlingoTransitionFrameSettings settings)
    {
        if (Member == null)
        {
            Member = _castlib.Add<BlingoTransitionMember>(0, settings.TransitionName + "_" + settings.Duration);
            Member.UsedBy(this);
        }
        else
            Member.Name = settings.TransitionName + "_" + settings.Duration;
        Member.SetSettings(settings);
    }

    public BlingoTransitionFrameSettings? GetSettings()
    {
        if (Member == null) return null;
        return Member.GetSettings();
    }

    public override Action<BlingoSprite> GetCloneAction()
    {
        var baseAction = base.GetCloneAction();
        Action<BlingoSprite> action = s => { };
        var settings = GetSettings();
        var member = Member;
        action = s =>
        {
            baseAction(s);
            var sprite = (BlingoTransitionSprite)s;
            sprite.Member = member;
            member?.UsedBy(sprite);
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }

    public void SetMember(BlingoTransitionMember transitionMember)
    {
        Member?.ReleaseFromRefUser(this);
        Member = transitionMember;
       Member.UsedBy(this);
       SetSettings(transitionMember.GetSettings());
    }

    public IBlingoMember? GetMember() => Member;

    public void MemberHasBeenRemoved()
    {
        Member = null;
    }
}

