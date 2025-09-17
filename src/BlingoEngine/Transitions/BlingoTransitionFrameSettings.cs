using AbstUI.Primitives;

namespace BlingoEngine.Transitions;

public class BlingoTransitionFrameSettings
{
    public int TransitionId { get; set; }
    public string TransitionName { get; set; } = "";
    /// <summary>
    /// Duration in Seconds from 0 to 30 seconds
    /// </summary>
    public float Duration { get; set; }
    public float Smoothness { get; set; }
    public BlingoTransitionAffects Affects { get; set; } = BlingoTransitionAffects.ChangingAreaOnly;
    public ARect Rect { get; set; }
}


