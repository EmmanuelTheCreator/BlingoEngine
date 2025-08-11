using System;
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
        public static byte[] Apply(ReadOnlySpan<byte> rgba, LingoInkType ink, LingoColor transparentColor)
        {
            byte[] result = rgba.ToArray();
            var maxLength = result.Length - 3;
            switch (ink)
            {
                case LingoInkType.BackgroundTransparent:
                case LingoInkType.Matte:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        if (result[i] == transparentColor.R &&
                            result[i + 1] == transparentColor.G &&
                            result[i + 2] == transparentColor.B)
                        {
                            result[i + 3] = 0;
                        }
                    }
                    break;
                case LingoInkType.Blend:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        byte a = result[i + 3];
                        result[i] = (byte)(result[i] * a / 255);
                        result[i + 1] = (byte)(result[i + 1] * a / 255);
                        result[i + 2] = (byte)(result[i + 2] * a / 255);
                    }
                    break;
                case LingoInkType.Reverse:
                case LingoInkType.NotCopy:
                case LingoInkType.NotReverse:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                    }
                    break;
                case LingoInkType.Ghost:
                case LingoInkType.NotGhost:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    }
                    break;
                case LingoInkType.Mask:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        result[i] = (byte)(255 - result[i]);
                        result[i + 1] = (byte)(255 - result[i + 1]);
                        result[i + 2] = (byte)(255 - result[i + 2]);
                        result[i + 3] = (byte)(result[i + 3] / 2);
                    }
                    break;
                case LingoInkType.NotTransparent:
                    for (int i = 0; i < maxLength; i += 4)
                    {
                        result[i + 3] = 255;
                    }
                    break;
            }

            return result;
        }
    }
}
