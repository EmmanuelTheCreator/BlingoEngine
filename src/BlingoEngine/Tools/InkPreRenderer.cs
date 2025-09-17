using System;
using System.Security.Cryptography;
using AbstUI.Primitives;
using AbstUI.Tools;
using BlingoEngine.Primitives;

namespace BlingoEngine.Tools
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
        public static bool CanHandle(BlingoInkType ink) => ink switch
        {
            BlingoInkType.Blend or
            BlingoInkType.BackgroundTransparent or
            BlingoInkType.Reverse or
            BlingoInkType.NotCopy or
            BlingoInkType.NotReverse or
            BlingoInkType.Ghost or
            BlingoInkType.NotGhost or
            BlingoInkType.Mask or
            BlingoInkType.NotTransparent or
            BlingoInkType.Matte => true,
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
        public static byte[] Apply(ReadOnlySpan<byte> rgba, BlingoInkType ink, AColor transparentColor)
        {
            byte[] result = rgba.ToArray();
            int pixelCount = result.Length / 4;

            switch (ink)
            {
                case BlingoInkType.BackgroundTransparent:
                case BlingoInkType.Matte:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
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
                case BlingoInkType.Blend:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        byte a = result[i + 3];
                        result[i] = (byte)(result[i] * a / 255);
                        result[i + 1] = (byte)(result[i + 1] * a / 255);
                        result[i + 2] = (byte)(result[i + 2] * a / 255);
                        result[i + 3] = 255;
                    });
                    break;
                case BlingoInkType.Reverse:
                case BlingoInkType.NotCopy:
                case BlingoInkType.NotReverse:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                    });
                    break;
                case BlingoInkType.Ghost:
                case BlingoInkType.NotGhost:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    });
                    break;
                case BlingoInkType.Mask:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    });
                    break;
                case BlingoInkType.NotTransparent:
                    ParallelHelper.For(0, pixelCount, pixelCount, idx =>
                    {
                        int i = idx * 4;
                        result[i + 3] = 255;
                    });
                    break;
            }

            return result;
        }

        public static BlingoInkType GetInkCacheKey(BlingoInkType ink)
        {
            var inkKey = ink;
            switch (ink)
            {
                case BlingoInkType.BackgroundTransparent: return BlingoInkType.Matte;
                case BlingoInkType.Reverse:
                case BlingoInkType.NotCopy:
                case BlingoInkType.NotReverse: return BlingoInkType.Reverse;
                case BlingoInkType.Ghost:
                case BlingoInkType.NotGhost: return BlingoInkType.Ghost;
                default:
                    return ink;
            }
        }
    }
}

