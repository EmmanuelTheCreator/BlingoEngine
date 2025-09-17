using System;

namespace BlingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Moves the current frame away to reveal the target frame beneath it.
/// </summary>
public sealed class RevealTransition : BlingoBaseTransition
{
    private readonly RevealDirection _direction;

    public RevealTransition(int id, string name, string code, string description, RevealDirection direction)
        : base(id, name, code, description, BlingoTransitionCategory.Reveal)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = (byte[])to.Clone();

        int shiftX = (int)(width * progress);
        int shiftY = (int)(height * progress);

        switch (_direction)
        {
            case RevealDirection.Up:
                if (height - shiftY > 0)
                    Buffer.BlockCopy(from, shiftY * width * 4, dest, 0, (height - shiftY) * width * 4);
                break;
            case RevealDirection.Down:
                if (height - shiftY > 0)
                    Buffer.BlockCopy(from, 0, dest, shiftY * width * 4, (height - shiftY) * width * 4);
                break;
            case RevealDirection.Left:
                for (int y = 0; y < height; y++)
                {
                    int row = y * width * 4;
                    if (width - shiftX > 0)
                        Buffer.BlockCopy(from, row + shiftX * 4, dest, row, (width - shiftX) * 4);
                }
                break;
            case RevealDirection.Right:
                for (int y = 0; y < height; y++)
                {
                    int row = y * width * 4;
                    if (width - shiftX > 0)
                        Buffer.BlockCopy(from, row, dest, row + shiftX * 4, (width - shiftX) * 4);
                }
                break;
            case RevealDirection.UpRight:
                for (int y = 0; y < height - shiftY; y++)
                {
                    int srcOffset = ((y + shiftY) * width + shiftX) * 4;
                    int destOffset = y * width * 4;
                    int copyWidth = width - shiftX;
                    if (copyWidth > 0)
                        Buffer.BlockCopy(from, srcOffset, dest, destOffset, copyWidth * 4);
                }
                break;
            case RevealDirection.DownRight:
                for (int y = shiftY; y < height; y++)
                {
                    int srcOffset = ((y - shiftY) * width + shiftX) * 4;
                    int destOffset = y * width * 4;
                    int copyWidth = width - shiftX;
                    if (copyWidth > 0)
                        Buffer.BlockCopy(from, srcOffset, dest, destOffset, copyWidth * 4);
                }
                break;
            case RevealDirection.DownLeft:
                for (int y = shiftY; y < height; y++)
                {
                    int srcOffset = (y - shiftY) * width * 4;
                    int destOffset = y * width * 4 + shiftX * 4;
                    int copyWidth = width - shiftX;
                    if (copyWidth > 0)
                        Buffer.BlockCopy(from, srcOffset, dest, destOffset, copyWidth * 4);
                }
                break;
            case RevealDirection.UpLeft:
                for (int y = 0; y < height - shiftY; y++)
                {
                    int srcOffset = (y + shiftY) * width * 4;
                    int destOffset = y * width * 4 + shiftX * 4;
                    int copyWidth = width - shiftX;
                    if (copyWidth > 0)
                        Buffer.BlockCopy(from, srcOffset, dest, destOffset, copyWidth * 4);
                }
                break;
        }

        return dest;
    }
}

public enum RevealDirection
{
    Up,
    UpRight,
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft
}


