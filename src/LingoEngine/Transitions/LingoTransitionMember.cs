using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.Transitions;

public enum LingoTransitionAffects
{
    EntireStage,
    ChangingAreaOnly,
    Custom
}

public class LingoTransitionMember : LingoMember
{
    public int TransitionId { get; set; }
    public string TransitionName { get; set; } = "";
    /// <summary>
    /// Duration in Seconds from 0 to 30 seconds
    /// </summary>
    public float Duration { get; set; }
    public float Smoothness { get; set; }
    public LingoTransitionAffects Affects { get; set; } = LingoTransitionAffects.ChangingAreaOnly;
    public ARect Rect { get; set; }

    public LingoTransitionMember(LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
        : base(new LingoTransitionMemberFW(), LingoMemberType.Transition, cast, numberInCast, name, fileName, regPoint)
    {
    }

    public void SetSettings(LingoTransitionFrameSettings settings)
    {
        TransitionId = settings.TransitionId;
        Affects = settings.Affects;
        Duration = settings.Duration;
        Smoothness = settings.Smoothness;
        TransitionName = settings.TransitionName;
        Rect = settings.Rect;
    }

    public LingoTransitionFrameSettings GetSettings()
    {
        return new LingoTransitionFrameSettings
        {
            Affects = Affects,
            TransitionId = TransitionId,
            Duration = Duration,
            Smoothness = Smoothness,
            TransitionName = TransitionName,
            Rect = Rect,
        };
    }
}

