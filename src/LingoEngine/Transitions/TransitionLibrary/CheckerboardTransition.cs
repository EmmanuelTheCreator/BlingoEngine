using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Reveals the target frame in a checkerboard pattern.
/// </summary>
public sealed class CheckerboardTransition : LingoBaseTransition
{
    private const int CheckerCount = 16;

    public CheckerboardTransition(int id, string name, string code, string description)
        : base(id, name, code, description, LingoTransitionCategory.Checkerboard)
    {
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];
        Array.Copy(from, dest, from.Length);

        int cellW = Math.Max(1, width / CheckerCount);
        int cellH = Math.Max(1, height / CheckerCount);
        int total = CheckerCount * CheckerCount;
        int reveal = (int)(total * progress);

        for (int idx = 0; idx < reveal; idx++)
        {
            int n = idx < total / 2 ? idx * 2 : (idx - total / 2) * 2 + 1;
            int cx = n % CheckerCount;
            int cy = n / CheckerCount;
            int xStart = cx * cellW;
            int yStart = cy * cellH;
            int copyWidth = Math.Min(cellW, width - xStart);
            int copyHeight = Math.Min(cellH, height - yStart);
            for (int y = 0; y < copyHeight; y++)
            {
                int srcOffset = ((yStart + y) * width + xStart) * 4;
                Buffer.BlockCopy(to, srcOffset, dest, srcOffset, copyWidth * 4);
            }
        }

        return dest;
    }
}
