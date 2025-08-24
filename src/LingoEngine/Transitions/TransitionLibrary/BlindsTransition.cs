using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Simple venetian blind style transition.
/// </summary>
public sealed class BlindsTransition : LingoBaseTransition
{
    private readonly BlindOrientation _orientation;
    private const int BlindCount = 12;

    public BlindsTransition(int id, string name, string code, string description, BlindOrientation orientation)
        : base(id, name, code, description)
    {
        _orientation = orientation;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        var dest = new byte[from.Length];
        Array.Copy(from, dest, from.Length);

        progress = Math.Clamp(progress, 0f, 1f);

        if (_orientation == BlindOrientation.Horizontal)
        {
            int blindHeight = Math.Max(1, height / BlindCount);
            int open = (int)(blindHeight * progress);
            for (int b = 0; b < BlindCount; b++)
            {
                int yStart = b * blindHeight;
                int yEnd = Math.Min(yStart + open, height);
                for (int y = yStart; y < yEnd; y++)
                {
                    int offset = y * width * 4;
                    Buffer.BlockCopy(to, offset, dest, offset, width * 4);
                }
            }
        }
        else
        {
            int blindWidth = Math.Max(1, width / BlindCount);
            int open = (int)(blindWidth * progress);
            for (int b = 0; b < BlindCount; b++)
            {
                int xStart = b * blindWidth;
                int xEnd = Math.Min(xStart + open, width);
                for (int y = 0; y < height; y++)
                {
                    int row = y * width * 4;
                    for (int x = xStart; x < xEnd; x++)
                    {
                        int i = row + x * 4;
                        dest[i] = to[i];
                        dest[i + 1] = to[i + 1];
                        dest[i + 2] = to[i + 2];
                        dest[i + 3] = to[i + 3];
                    }
                }
            }
        }

        return dest;
    }
}

public enum BlindOrientation
{
    Horizontal,
    Vertical
}

