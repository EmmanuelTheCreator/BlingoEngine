using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Transition that reveals the target frame a strip at a time.
/// </summary>
public sealed class StripsTransition : LingoBaseTransition
{
    private readonly StripsDirection _direction;
    private const int StripCount = 16;

    public StripsTransition(int id, string name, string code, string description, StripsDirection direction)
        : base(id, name, code, description, LingoTransitionCategory.Strips)
    {
        _direction = direction;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = new byte[from.Length];
        Array.Copy(from, dest, from.Length);

        switch (_direction)
        {
            case StripsDirection.LeftToRight:
            case StripsDirection.RightToLeft:
                {
                    int stripWidth = Math.Max(1, width / StripCount);
                    int stripsToReveal = (int)(StripCount * progress);
                    for (int s = 0; s < stripsToReveal; s++)
                    {
                        int index = _direction == StripsDirection.LeftToRight ? s : StripCount - 1 - s;
                        int xStart = index * stripWidth;
                        int copyWidth = Math.Min(stripWidth, width - xStart);
                        for (int y = 0; y < height; y++)
                        {
                            int offset = (y * width + xStart) * 4;
                            Buffer.BlockCopy(to, offset, dest, offset, copyWidth * 4);
                        }
                    }
                    break;
                }
            case StripsDirection.TopToBottom:
            case StripsDirection.BottomToTop:
                {
                    int stripHeight = Math.Max(1, height / StripCount);
                    int stripsToReveal = (int)(StripCount * progress);
                    for (int s = 0; s < stripsToReveal; s++)
                    {
                        int index = _direction == StripsDirection.TopToBottom ? s : StripCount - 1 - s;
                        int yStart = index * stripHeight;
                        int copyHeight = Math.Min(stripHeight, height - yStart);
                        int offset = yStart * width * 4;
                        Buffer.BlockCopy(to, offset, dest, offset, copyHeight * width * 4);
                    }
                    break;
                }
        }

        return dest;
    }
}

public enum StripsDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}

