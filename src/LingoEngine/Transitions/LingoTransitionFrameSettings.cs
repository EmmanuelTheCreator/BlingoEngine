using AbstUI.Primitives;

namespace LingoEngine.Transitions;

public class LingoTransitionFrameSettings
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
}

