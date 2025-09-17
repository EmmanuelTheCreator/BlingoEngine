using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Transitions;

public enum BlingoTransitionAffects
{
    EntireStage,
    ChangingAreaOnly,
    Custom
}

public class BlingoTransitionMember : BlingoMember
{
    private int _transitionId;
    private string _transitionName = "";
    /// <summary>
    /// Duration in Seconds from 0 to 30 seconds
    /// </summary>
    private float _duration;
    private float _smoothness;
    private BlingoTransitionAffects _affects = BlingoTransitionAffects.ChangingAreaOnly;
    private ARect _rect;

    public int TransitionId
    {
        get => _transitionId;
        set => SetProperty(ref _transitionId, value);
    }

    public string TransitionName
    {
        get => _transitionName;
        set => SetProperty(ref _transitionName, value);
    }

    public float Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    public float Smoothness
    {
        get => _smoothness;
        set => SetProperty(ref _smoothness, value);
    }

    public BlingoTransitionAffects Affects
    {
        get => _affects;
        set => SetProperty(ref _affects, value);
    }

    public ARect Rect
    {
        get => _rect;
        set => SetProperty(ref _rect, value);
    }

    public BlingoTransitionMember(BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
        : base(new BlingoTransitionMemberFW(), BlingoMemberType.Transition, cast, numberInCast, name, fileName, regPoint)
    {
    }

    public void SetSettings(BlingoTransitionFrameSettings settings)
    {
        TransitionId = settings.TransitionId;
        Affects = settings.Affects;
        Duration = settings.Duration;
        Smoothness = settings.Smoothness;
        TransitionName = settings.TransitionName;
        Rect = settings.Rect;
    }

    public BlingoTransitionFrameSettings GetSettings()
    {
        return new BlingoTransitionFrameSettings
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


