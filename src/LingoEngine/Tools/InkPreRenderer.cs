using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AbstUI.Primitives;
using LingoEngine.Primitives;

namespace LingoEngine.Tools
{
    /// <summary>
    /// Utility helper to pre-render simple ink effects directly on pixel data.
    /// </summary>
    public static class InkPreRenderer
    {
        /// <summary>
        /// Returns <c>true</c> if the given ink type can be pre-rendered without
        /// reading the destination buffer.
        /// </summary>
        public static bool CanHandle(LingoInkType ink) => ink switch
        {
            LingoInkType.Blend or
            LingoInkType.BackgroundTransparent or
            LingoInkType.Reverse or
            LingoInkType.NotCopy or
            LingoInkType.NotReverse or
            LingoInkType.Ghost or
            LingoInkType.NotGhost or
            LingoInkType.Mask or
            LingoInkType.NotTransparent or
            LingoInkType.Matte => true,
            _ => false,
        };
        /// <summary>
        /// Applies the given ink effect to an RGBA pixel buffer and returns a new array.
        /// Handles ink types that do not require knowledge of the destination
        /// buffer, allowing preprocessing of the bitmap.
        /// </summary>
        /// <param name="rgba">Source pixels in RGBA8888 format.</param>
        /// <param name="ink">Ink type to apply.</param>
        /// <param name="transparentColor">Stage background color used for <c>BackgroundTransparent</c>.</param>
        public static byte[] Apply(ReadOnlySpan<byte> rgba, LingoInkType ink, AColor transparentColor)
        {
            byte[] result = rgba.ToArray();
            int pixelCount = result.Length / 4;

            switch (ink)
            {
                case LingoInkType.BackgroundTransparent:
                case LingoInkType.Matte:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        if (result[i] == transparentColor.R &&
                            result[i + 1] == transparentColor.G &&
                            result[i + 2] == transparentColor.B)
                        {
                            result[i + 3] = 0;
                        }
                    });
                    break;
                case LingoInkType.Blend:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        byte a = result[i + 3];
                        result[i] = (byte)(result[i] * a / 255);
                        result[i + 1] = (byte)(result[i + 1] * a / 255);
                        result[i + 2] = (byte)(result[i + 2] * a / 255);
                        result[i + 3] = 255;
                    });
                    break;
                case LingoInkType.Reverse:
                case LingoInkType.NotCopy:
                case LingoInkType.NotReverse:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                    });
                    break;
                case LingoInkType.Ghost:
                case LingoInkType.NotGhost:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    });
                    break;
                case LingoInkType.Mask:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    });
                    break;
                case LingoInkType.NotTransparent:
                    Parallel.For(0, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i + 3] = 255;
                    });
                    break;
            }

            return result;
        }

        public static LingoInkType GetInkCacheKey(LingoInkType ink)
        {
            var inkKey = ink;
            switch (ink)
            {
                case LingoInkType.BackgroundTransparent:return LingoInkType.Matte;
                case LingoInkType.Reverse:
                case LingoInkType.NotCopy:
                case LingoInkType.NotReverse:return LingoInkType.Reverse;
                case LingoInkType.Ghost:
                case LingoInkType.NotGhost: return LingoInkType.Ghost;
                default:
                    return ink;
            }
        }
    }
}
