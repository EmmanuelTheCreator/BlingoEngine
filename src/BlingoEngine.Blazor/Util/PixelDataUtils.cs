namespace BlingoEngine.Blazor.Util;

internal static class PixelDataUtils
{
    public static bool IsTransparent(byte[]? data, int stride, int width, int height, int x, int y)
    {
        if (data == null || x < 0 || y < 0 || x >= width || y >= height)
            return false;
        int index = y * stride + x * 4 + 3; // alpha channel
        return data[index] == 0;
    }
}

