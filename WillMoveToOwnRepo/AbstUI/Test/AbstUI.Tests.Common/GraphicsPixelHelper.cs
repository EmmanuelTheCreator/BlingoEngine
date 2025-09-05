namespace AbstUI.Tests.Common;

public static class GraphicsPixelHelper
{
    public static int FindTopOpaqueRow(byte[] rgbaPixels, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4 + 3;
                if (rgbaPixels[index] > 0)
                    return y;
            }
        }
        return -1;
    }

    public static int FindBottomOpaqueRow(byte[] rgbaPixels, int width, int height)
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4 + 3;
                if (rgbaPixels[index] > 0)
                    return y;
            }
        }
        return -1;
    }
}
