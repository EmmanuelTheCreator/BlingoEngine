using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Simple box in/out transition.
/// </summary>
public sealed class BoxTransition : LingoBaseTransition
{
    private readonly BoxDirection _direction;

    public BoxTransition(int id, string name, string code, string description, BoxDirection direction)
        : base(id, name, code, description, LingoTransitionCategory.Box)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];

        if (_direction == BoxDirection.Out)
        {
            Array.Copy(from, dest, from.Length);
            int w = (int)(width * progress);
            int h = (int)(height * progress);
            int x = (width - w) / 2;
            int y = (height - h) / 2;
            for (int row = 0; row < h; row++)
            {
                int srcOffset = ((y + row) * width + x) * 4;
                Buffer.BlockCopy(to, srcOffset, dest, srcOffset, w * 4);
            }
        }
        else
        {
            Array.Copy(to, dest, to.Length);
            int w = (int)(width * (1f - progress));
            int h = (int)(height * (1f - progress));
            int x = (width - w) / 2;
            int y = (height - h) / 2;
            if (w > 0 && h > 0)
            {
                for (int row = 0; row < h; row++)
                {
                    int srcOffset = ((y + row) * width + x) * 4;
                    Buffer.BlockCopy(from, srcOffset, dest, srcOffset, w * 4);
                }
            }
        }

        return dest;
    }
}

public enum BoxDirection
{
    Out,
    In
}

