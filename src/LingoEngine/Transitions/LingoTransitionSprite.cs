using LingoEngine.ColorPalettes;
using LingoEngine.Movies;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public class LingoTransitionSprite : LingoSprite
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
        _removeMe(this);
    }
    public void SetSettings(LingoTransitionFrameSettings settings)
    {
        if (Member == null)
            Member = _environment.CastLibsContainer.ActiveCast.Add<LingoTransitionMember>(0, "");
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
            sprite.Member = Member;
            if (settings != null)
                sprite.SetSettings(settings);
        };

        return action;
    }

    public void SetMember(LingoTransitionMember transitionMember)
    {
        Member = transitionMember;
        SetSettings(transitionMember.GetSettings());
    }
}
