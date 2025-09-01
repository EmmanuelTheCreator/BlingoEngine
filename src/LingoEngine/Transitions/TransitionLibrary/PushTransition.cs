using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Slides the current frame out while the target frame pushes in from the opposite side.
/// </summary>
public sealed class PushTransition : LingoBaseTransition
{
    private readonly PushDirection _direction;

    public PushTransition(int id, string name, string code, string description, PushDirection direction)
        : base(id, name, code, description, LingoTransitionCategory.Push)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];

        int shiftX = 0;
        int shiftY = 0;

        switch (_direction)
        {
            case PushDirection.Left:
                shiftX = (int)(width * progress);
                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * width * 4;
                    int remaining = width - shiftX;
                    if (remaining > 0)
                        Buffer.BlockCopy(from, rowOffset + shiftX * 4, dest, rowOffset, remaining * 4);
                    if (shiftX > 0)
                        Buffer.BlockCopy(to, rowOffset + (width - shiftX) * 4, dest, rowOffset + remaining * 4, shiftX * 4);
                }
                break;
            case PushDirection.Right:
                shiftX = (int)(width * progress);
                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * width * 4;
                    int remaining = width - shiftX;
                    if (remaining > 0)
                        Buffer.BlockCopy(from, rowOffset, dest, rowOffset + shiftX * 4, remaining * 4);
                    if (shiftX > 0)
                        Buffer.BlockCopy(to, rowOffset, dest, rowOffset, shiftX * 4);
                }
                break;
            case PushDirection.Down:
                shiftY = (int)(height * progress);
                if (shiftY > 0)
                    Buffer.BlockCopy(to, 0, dest, 0, shiftY * width * 4);
                if (height - shiftY > 0)
                    Buffer.BlockCopy(from, 0, dest, shiftY * width * 4, (height - shiftY) * width * 4);
                break;
            case PushDirection.Up:
                shiftY = (int)(height * progress);
                if (height - shiftY > 0)
                    Buffer.BlockCopy(from, shiftY * width * 4, dest, 0, (height - shiftY) * width * 4);
                if (shiftY > 0)
                    Buffer.BlockCopy(to, (height - shiftY) * width * 4, dest, (height - shiftY) * width * 4, shiftY * width * 4);
                break;
        }

        return dest;
    }
}

public enum PushDirection
{
    Left,
    Right,
    Down,
    Up
}

