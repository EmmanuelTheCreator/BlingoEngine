using System;

namespace AbstUI.Tools;

/// <summary>
/// Utility methods for pixel data manipulation.
/// </summary>
public static class APixel
{
    /// <summary>
    /// Converts a pixel buffer from ARGB to RGBA in place.
    /// </summary>
    public static void ToRGBA(byte[] argbPixels)
    {
        int pixelCount = argbPixels.Length / 4;
        ParallelHelper.For(0, pixelCount, pixelCount, i =>
        {
            int idx = i * 4;
            byte a = argbPixels[idx];
            byte r = argbPixels[idx + 1];
            byte g = argbPixels[idx + 2];
            byte b = argbPixels[idx + 3];
            argbPixels[idx] = r;
            argbPixels[idx + 1] = g;
            argbPixels[idx + 2] = b;
            argbPixels[idx + 3] = a;
        });
    }
}
