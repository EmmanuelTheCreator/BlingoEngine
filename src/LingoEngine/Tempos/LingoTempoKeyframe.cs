using LingoEngine.Animations;

namespace LingoEngine.Tempos;

public class LingoTempoKeyframe : LingoKeyFrame<int>
{
    public int Fps { get; set; }
    public LingoTempoKeyframe(int frame, int value) : base(frame, value)
    {
    }


    

}
