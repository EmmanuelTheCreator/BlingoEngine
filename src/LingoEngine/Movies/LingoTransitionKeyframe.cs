namespace LingoEngine.Movies;

public class LingoTransitionKeyframe : ILingoKeyframe
{
    public int Frame { get; set; }
    public int TransitionId { get; set; }

    public LingoTransitionKeyframe(int frame, int transitionId)
    {
        Frame = frame;
        TransitionId = transitionId;
    }
}
