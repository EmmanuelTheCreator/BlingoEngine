using BlingoEngine.Animations;

namespace BlingoEngine.Tempos;

public class BlingoTempoKeyframe : BlingoKeyFrame<int>
{
    public int Fps { get; set; }
    public BlingoTempoKeyframe(int frame, int value) : base(frame, value)
    {
    }


    

}

