using AbstUI.Tools;

namespace LingoEngine.Transitions.TransitionLibrary;

public sealed class FadeTransition : LingoBaseTransition
{
    public FadeTransition() : base(1, "Fade", "fade", "Linear fade between frames")
    {
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        var length = from.Length;
        var dest = new byte[length];
        int pixelCount = length / 4;
        ParallelHelper.For(0, pixelCount, length, idx =>
        {
            int i = idx * 4;

            byte aF = from[i];
            byte rF = from[i + 1];
            byte gF = from[i + 2];
            byte bF = from[i + 3];

            byte aT = to[i];
            byte rT = to[i + 1];
            byte gT = to[i + 2];
            byte bT = to[i + 3];

            dest[i] = (byte)(aF + (aT - aF) * progress);
            dest[i + 1] = (byte)(rF + (rT - rF) * progress);
            dest[i + 2] = (byte)(gF + (gT - gF) * progress);
            dest[i + 3] = (byte)(bF + (bT - bF) * progress);
        });
        return dest;
    }
}
