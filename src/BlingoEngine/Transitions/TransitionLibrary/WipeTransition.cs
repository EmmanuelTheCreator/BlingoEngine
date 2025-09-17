using System;

namespace BlingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Simple wipe transition that reveals the target frame in one of four directions.
/// </summary>
public sealed class WipeTransition : BlingoBaseTransition
{
    private readonly WipeDirection _direction;

    public WipeTransition(int id, string name, string code, string description, WipeDirection direction)
        : base(id, name, code, description, BlingoTransitionCategory.Wipe)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        var length = from.Length;
        var dest = new byte[length];
        Array.Copy(from, dest, length);

        progress = MathCompat.Clamp(progress, 0f, 1f);

        switch (_direction)
        {
            case WipeDirection.Right:
                {
                    int wipeWidth = (int)(width * progress);
                    for (int y = 0; y < height; y++)
                    {
                        int row = y * width * 4;
                        for (int x = 0; x < wipeWidth; x++)
                        {
                            int i = row + x * 4;
                            dest[i] = to[i];
                            dest[i + 1] = to[i + 1];
                            dest[i + 2] = to[i + 2];
                            dest[i + 3] = to[i + 3];
                        }
                    }
                    break;
                }
            case WipeDirection.Left:
                {
                    int wipeWidth = (int)(width * progress);
                    int startX = width - wipeWidth;
                    for (int y = 0; y < height; y++)
                    {
                        int row = y * width * 4;
                        for (int x = startX; x < width; x++)
                        {
                            int i = row + x * 4;
                            dest[i] = to[i];
                            dest[i + 1] = to[i + 1];
                            dest[i + 2] = to[i + 2];
                            dest[i + 3] = to[i + 3];
                        }
                    }
                    break;
                }
            case WipeDirection.Down:
                {
                    int wipeHeight = (int)(height * progress);
                    for (int y = 0; y < wipeHeight; y++)
                    {
                        int offset = y * width * 4;
                        Buffer.BlockCopy(to, offset, dest, offset, width * 4);
                    }
                    break;
                }
            case WipeDirection.Up:
                {
                    int wipeHeight = (int)(height * progress);
                    int startY = height - wipeHeight;
                    for (int y = startY; y < height; y++)
                    {
                        int offset = y * width * 4;
                        Buffer.BlockCopy(to, offset, dest, offset, width * 4);
                    }
                    break;
                }
        }

        return dest;
    }
}

public enum WipeDirection
{
    Right,
    Left,
    Down,
    Up
}

