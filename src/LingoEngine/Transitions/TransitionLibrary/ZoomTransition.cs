using System;

namespace LingoEngine.Transitions.TransitionLibrary;

/// <summary>
/// Performs a simple zoom in or zoom out effect around the center of the frame.
/// </summary>
public sealed class ZoomTransition : LingoBaseTransition
{
    private readonly bool _open;

    public ZoomTransition(int id, string name, string code, string description, bool open)
        : base(id, name, code, description, LingoTransitionCategory.Zoom)
    {
        _open = open;
    }

    public override byte[] StepFrame(int width, int height, byte[] from, byte[] to, float progress)
    {
        progress = MathCompat.Clamp(progress, 0f, 1f);
        var dest = _open ? (byte[])from.Clone() : (byte[])to.Clone();
        var source = _open ? to : from;

        float scale = _open ? progress : 1f - progress;
        if (scale <= 0f)
            return dest;

        int rectW = (int)(width * scale);
        int rectH = (int)(height * scale);
        if (rectW <= 0 || rectH <= 0)
            return dest;

        int startX = (width - rectW) / 2;
        int startY = (height - rectH) / 2;
        float scaleX = (float)width / rectW;
        float scaleY = (float)height / rectH;

        for (int y = 0; y < rectH; y++)
        {
            int destY = startY + y;
            int destRow = (destY * width + startX) * 4;
            int srcY = (int)(y * scaleY);
            for (int x = 0; x < rectW; x++)
            {
                int srcX = (int)(x * scaleX);
                int srcIndex = (srcY * width + srcX) * 4;
                int destIndex = destRow + x * 4;
                dest[destIndex] = source[srcIndex];
                dest[destIndex + 1] = source[srcIndex + 1];
                dest[destIndex + 2] = source[srcIndex + 2];
                dest[destIndex + 3] = source[srcIndex + 3];
            }
        }

        return dest;
    }
}

