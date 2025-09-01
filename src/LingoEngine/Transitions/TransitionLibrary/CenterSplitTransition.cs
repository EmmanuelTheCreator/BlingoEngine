using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Reveals or hides the target frame from the center either horizontally or vertically.
/// </summary>
public sealed class CenterSplitTransition : LingoBaseTransition
{
    private readonly SplitOrientation _orientation;
    private readonly SplitDirection _direction;

    public CenterSplitTransition(int id, string name, string code, string description, SplitOrientation orientation, SplitDirection direction)
        : base(id, name, code, description, LingoTransitionCategory.Split)
    {
        _orientation = orientation;
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        byte[] dest = _direction == SplitDirection.Out ? (byte[])from.Clone() : (byte[])to.Clone();

        if (_orientation == SplitOrientation.Horizontal)
        {
            int half = width / 2;
            int reveal = _direction == SplitDirection.Out ? (int)(half * progress) : (int)(half * (1f - progress));
            int xStart = half - reveal;
            int copyWidth = reveal * 2;
            if (copyWidth > 0)
            {
                for (int y = 0; y < height; y++)
                {
                    int offset = (y * width + xStart) * 4;
                    Buffer.BlockCopy(_direction == SplitDirection.Out ? to : from, offset, dest, offset, copyWidth * 4);
                }
            }
        }
        else
        {
            int half = height / 2;
            int reveal = _direction == SplitDirection.Out ? (int)(half * progress) : (int)(half * (1f - progress));
            int yStart = half - reveal;
            int copyHeight = reveal * 2;
            if (copyHeight > 0)
            {
                int srcBase = yStart * width * 4;
                Buffer.BlockCopy(_direction == SplitDirection.Out ? to : from, srcBase, dest, srcBase, copyHeight * width * 4);
            }
        }

        return dest;
    }
}

public enum SplitOrientation
{
    Horizontal,
    Vertical
}

public enum SplitDirection
{
    Out,
    In
}
