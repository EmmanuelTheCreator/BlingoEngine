using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Randomly dissolves the source frame into the target.
/// </summary>
public sealed class DissolveTransition : LingoBaseTransition
{
    public DissolveTransition(int id, string name, string code, string description)
        : base(id, name, code, description)
    {
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];
        int pixelCount = width * height;
        for (int p = 0; p < pixelCount; p++)
        {
            int x = p % width;
            int y = p / width;
            uint hash = (uint)((x * 73856093) ^ (y * 19349663));
            double val = hash / (double)uint.MaxValue;
            int idx = p * 4;
            if (val < progress)
            {
                dest[idx] = to[idx];
                dest[idx + 1] = to[idx + 1];
                dest[idx + 2] = to[idx + 2];
                dest[idx + 3] = to[idx + 3];
            }
            else
            {
                dest[idx] = from[idx];
                dest[idx + 1] = from[idx + 1];
                dest[idx + 2] = from[idx + 2];
                dest[idx + 3] = from[idx + 3];
            }
        }
        return dest;
    }
}
