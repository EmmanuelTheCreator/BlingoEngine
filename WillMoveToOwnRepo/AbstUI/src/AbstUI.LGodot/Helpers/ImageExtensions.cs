using Godot;
using AbstUI.Tools;

namespace AbstUI.LGodot.Helpers
{
    /// <summary>
    /// Image extension helpers for the Godot backend.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Blends the source image onto <paramref name="dest"/> using the provided
        /// transform and alpha multiplier.
        /// </summary>
        /// <param name="src">Source image to blend.</param>
        /// <param name="dest">Destination image that receives the blended pixels.</param>
        /// <param name="transform">Affine transform applied to the source image.</param>
        /// <param name="alpha">Overall alpha multiplier in range [0,1].</param>
        public unsafe static void BlendImageTo(this Image src, Image dest, Transform2D transform, float alpha)
        {
            var inv = transform.AffineInverse();
            Vector2[] pts =
            {
                transform * Vector2.Zero,
                transform * new Vector2(src.GetWidth(), 0),
                transform * new Vector2(src.GetWidth(), src.GetHeight()),
                transform * new Vector2(0, src.GetHeight())
            };
            int minX = (int)MathF.Floor(pts.Min(p => p.X));
            int maxX = (int)MathF.Ceiling(pts.Max(p => p.X));
            int minY = (int)MathF.Floor(pts.Min(p => p.Y));
            int maxY = (int)MathF.Ceiling(pts.Max(p => p.Y));

            int destWidth = dest.GetWidth();
            int destHeight = dest.GetHeight();
            int srcWidth = src.GetWidth();
            int srcHeight = src.GetHeight();

            var destData = dest.GetData();
            var srcData = src.GetData();

            int destPitch = destWidth * 4;
            int srcPitch = srcWidth * 4;

            fixed (byte* pDestFixed = destData)
            fixed (byte* pSrcFixed = srcData)
            {
                IntPtr destPtr = (IntPtr)pDestFixed;
                IntPtr srcPtr = (IntPtr)pSrcFixed;

                int totalPixels = (maxX - minX) * (maxY - minY);
                ParallelHelper.For(minY, maxY, totalPixels, y =>
                {
                    if ((uint)y >= (uint)destHeight) return;

                    byte* pDest = (byte*)destPtr;
                    byte* pSrc = (byte*)srcPtr;

                    int destRow = y * destPitch;

                    for (int x = minX; x < maxX; x++)
                    {
                        if ((uint)x >= (uint)destWidth) continue;

                        var srcPos = inv * new Vector2(x + 0.5f, y + 0.5f);
                        int sx = (int)MathF.Floor(srcPos.X);
                        int sy = (int)MathF.Floor(srcPos.Y);
                        if ((uint)sx >= (uint)srcWidth || (uint)sy >= (uint)srcHeight)
                            continue;

                        int srcIndex = sy * srcPitch + sx * 4;
                        int destIndex = destRow + x * 4;

                        float a = pSrc[srcIndex + 3] / 255f * alpha;
                        if (a <= 0f) continue;
                        float invA = 1f - a;

                        pDest[destIndex] = (byte)(pSrc[srcIndex] * a + pDest[destIndex] * invA);
                        pDest[destIndex + 1] = (byte)(pSrc[srcIndex + 1] * a + pDest[destIndex + 1] * invA);
                        pDest[destIndex + 2] = (byte)(pSrc[srcIndex + 2] * a + pDest[destIndex + 2] * invA);
                        pDest[destIndex + 3] = (byte)(pSrc[srcIndex + 3] * a + pDest[destIndex + 3] * invA);
                    }
                });
            }

            dest.SetData(destWidth, destHeight, false, Image.Format.Rgba8, destData);
        }
    }
}
