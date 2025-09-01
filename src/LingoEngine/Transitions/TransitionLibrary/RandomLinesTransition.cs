using System;
using System.Linq;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Reveals the target frame in random horizontal or vertical lines.
/// </summary>
public sealed class RandomLinesTransition : LingoBaseTransition
{
    private readonly RandomLineOrientation _orientation;

    public RandomLinesTransition(int id, string name, string code, string description, RandomLineOrientation orientation)
        : base(id, name, code, description, LingoTransitionCategory.Random)
    {
        _orientation = orientation;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = (byte[])from.Clone();

        int lines = _orientation == RandomLineOrientation.Horizontal ? height : width;
        int reveal = (int)(lines * progress);
        if (reveal <= 0)
            return dest;

        var order = Enumerable.Range(0, lines).ToArray();
        var rng = new Random(0);
        for (int i = lines - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        for (int i = 0; i < reveal; i++)
        {
            int index = order[i];
            if (_orientation == RandomLineOrientation.Horizontal)
            {
                int offset = index * width * 4;
                Buffer.BlockCopy(to, offset, dest, offset, width * 4);
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    int src = (y * width + index) * 4;
                    dest[src] = to[src];
                    dest[src + 1] = to[src + 1];
                    dest[src + 2] = to[src + 2];
                    dest[src + 3] = to[src + 3];
                }
            }
        }

        return dest;
    }
}

public enum RandomLineOrientation
{
    Horizontal,
    Vertical
}

