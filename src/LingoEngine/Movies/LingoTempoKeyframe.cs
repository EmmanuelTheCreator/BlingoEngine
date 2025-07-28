namespace LingoEngine.Movies;

public class LingoTempoKeyframe : ILingoKeyframe
{
    public int Frame { get; set; }
    public int Fps { get; set; }

    public LingoTempoKeyframe(int frame, int fps)
    {
        Frame = frame;
        Fps = fps;
    }
}
