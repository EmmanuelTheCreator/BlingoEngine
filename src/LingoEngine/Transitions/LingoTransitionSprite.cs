using LingoEngine.ColorPalettes;
using LingoEngine.Movies;
using LingoEngine.Sprites;
using LingoEngine.Members;

namespace LingoEngine.Transitions;

public class LingoTransitionSprite : LingoSprite, ILingoSpriteWithMember
{
    public const int SpriteNumOffset = 2;
    private readonly Action<LingoTransitionSprite> _removeMe;

    public int Frame { get; set; }
    public int TransitionId { get; set; }

    public LingoTransitionMember? Member { get; set; }
    public override int SpriteNumWithChannel => SpriteNumOffset + SpriteNum;

    public LingoTransitionSprite(ILingoMovieEnvironment environment, Action<LingoTransitionSprite> removeMe) : base(environment)
    {
        _removeMe = removeMe;
        IsSingleFrame = true;
    }

    public override void OnRemoveMe()
    {
        Member?.ReleaseFromRefUser(this);
        _removeMe(this);
    }
    public void SetSettings(LingoTransitionFrameSettings settings)
    {
        if (Member == null)
        {
            Member = _environment.CastLibsContainer.ActiveCast.Add<LingoTransitionMember>(0, "");
            Member.UsedBy(this);
        }
        Member.SetSettings(settings);
    }

    public LingoTransitionFrameSettings? GetSettings()
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
            var sprite = (LingoTransitionSprite)s;
            sprite.Member = member;
            member?.UsedBy(sprite);
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }

    public void SetMember(LingoTransitionMember transitionMember)
    {
        Member?.ReleaseFromRefUser(this);
        Member = transitionMember;
       Member.UsedBy(this);
       SetSettings(transitionMember.GetSettings());
    }

    public ILingoMember? GetMember() => Member;

    public void MemberHasBeenRemoved()
    {
        Member = null;
    }
}
