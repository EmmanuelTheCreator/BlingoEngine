using System;
using AbstUI.Primitives;

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

    /// <summary>
    /// Copies a rectangular region of pixels from <paramref name="src"/> to <paramref name="dest"/>.
    /// </summary>
    public static void CopyRectPixels(byte[] src, byte[] dest, int width, ARect rect)
    {
        int x = (int)rect.Left;
        int y = (int)rect.Top;
        int w = (int)rect.Width;
        int h = (int)rect.Height;
        for (int row = 0; row < h; row++)
        {
            int srcIdx = ((y + row) * width + x) * 4;
            Buffer.BlockCopy(src, srcIdx, dest, srcIdx, w * 4);
        }
    }

    /// <summary>
    /// Computes the smallest rectangle that contains all differing pixels between two buffers.
    /// </summary>
    public static ARect ComputeDifferenceRect(int width, int height, byte[] from, byte[] to)
    {
        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                int idx = rowIndex + x * 4;
                if (from[idx] != to[idx] || from[idx + 1] != to[idx + 1] || from[idx + 2] != to[idx + 2] || from[idx + 3] != to[idx + 3])
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }
        if (maxX < minX || maxY < minY)
            return ARect.New(0, 0, width, height);
        return ARect.New(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
