using System;

namespace BlingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Slides the target frame over the source from a given direction.
/// </summary>
public sealed class CoverTransition : BlingoBaseTransition
{
    private readonly CoverDirection _direction;

    public CoverTransition(int id, string name, string code, string description, CoverDirection direction)
        : base(id, name, code, description, BlingoTransitionCategory.Cover)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];
        Array.Copy(from, dest, from.Length);

        int xOffset = 0;
        int yOffset = 0;

        switch (_direction)
        {
            case CoverDirection.Down:
                yOffset = (int)(progress * height) - height;
                break;
            case CoverDirection.Up:
                yOffset = height - (int)(progress * height);
                break;
            case CoverDirection.Left:
                xOffset = width - (int)(progress * width);
                break;
            case CoverDirection.Right:
                xOffset = (int)(progress * width) - width;
                break;
            case CoverDirection.DownLeft:
                xOffset = width - (int)(progress * width);
                yOffset = (int)(progress * height) - height;
                break;
            case CoverDirection.DownRight:
                xOffset = (int)(progress * width) - width;
                yOffset = (int)(progress * height) - height;
                break;
            case CoverDirection.UpLeft:
                xOffset = width - (int)(progress * width);
                yOffset = height - (int)(progress * height);
                break;
            case CoverDirection.UpRight:
                xOffset = (int)(progress * width) - width;
                yOffset = height - (int)(progress * height);
                break;
        }

        for (int y = 0; y < height; y++)
        {
            int destY = y + yOffset;
            if (destY < 0 || destY >= height)
                continue;

            int srcRow = y * width * 4;
            int destRow = destY * width * 4;

            int srcXStart = Math.Max(0, -xOffset);
            int destXStart = Math.Max(0, xOffset);
            int copyWidth = Math.Min(width - srcXStart, width - destXStart);
            if (copyWidth <= 0)
                continue;

            Buffer.BlockCopy(to, srcRow + srcXStart * 4, dest, destRow + destXStart * 4, copyWidth * 4);
        }

        return dest;
    }
}

public enum CoverDirection
{
    Down,
    DownLeft,
    DownRight,
    Left,
    Right,
    Up,
    UpLeft,
    UpRight
}

