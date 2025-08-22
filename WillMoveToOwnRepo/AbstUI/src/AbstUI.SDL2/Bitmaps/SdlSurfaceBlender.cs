using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Bitmaps;

/// <summary>
/// Utility functions for blending SDL surfaces.
/// </summary>
public static class SdlSurfaceBlender
{
    /// <summary>
    /// Blends <paramref name="src"/> onto <paramref name="dest"/> using an affine transform.
    /// </summary>
    public static unsafe void BlendSurface(nint dest, nint src, Matrix3x2 transform, float _)
    {
        SDL.SDL_LockSurface(dest);
        SDL.SDL_LockSurface(src);

        var dSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(dest);
        var sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(src);
        var dFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(dSurf.format);
        var sFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sSurf.format);

        if (dSurf.format == nint.Zero || sSurf.format == nint.Zero) goto UNLOCK;
        if (dFmt.BytesPerPixel != 4 || sFmt.BytesPerPixel != 4) goto UNLOCK;

        static void Unpack(uint px, in SDL.SDL_PixelFormat f, out byte r, out byte g, out byte b, out byte a)
        {
            r = (byte)((px & f.Rmask) >> f.Rshift);
            g = (byte)((px & f.Gmask) >> f.Gshift);
            b = (byte)((px & f.Bmask) >> f.Bshift);
            a = (byte)((px & f.Amask) >> f.Ashift);
            r = (byte)(r * 255 / (f.Rmask >> f.Rshift));
            g = (byte)(g * 255 / (f.Gmask >> f.Gshift));
            b = (byte)(b * 255 / (f.Bmask >> f.Bshift));
            a = (byte)(a * 255 / (f.Amask >> f.Ashift));
        }
        static uint Pack(byte r, byte g, byte b, byte a, in SDL.SDL_PixelFormat f)
        {
            uint R = r * (f.Rmask >> f.Rshift) / 255 << f.Rshift;
            uint G = g * (f.Gmask >> f.Gshift) / 255 << f.Gshift;
            uint B = b * (f.Bmask >> f.Bshift) / 255 << f.Bshift;
            uint A = a * (f.Amask >> f.Ashift) / 255 << f.Ashift;
            return R & f.Rmask | G & f.Gmask | B & f.Bmask | A & f.Amask;
        }

        Matrix3x2.Invert(transform, out var inv);

        Vector2[] pts =
        {
            Vector2.Transform(Vector2.Zero, transform),
            Vector2.Transform(new Vector2(sSurf.w, 0), transform),
            Vector2.Transform(new Vector2(sSurf.w, sSurf.h), transform),
            Vector2.Transform(new Vector2(0, sSurf.h), transform)
        };
        int minX = (int)MathF.Floor(pts.Min(p => p.X));
        int maxX = (int)MathF.Ceiling(pts.Max(p => p.X));
        int minY = (int)MathF.Floor(pts.Min(p => p.Y));
        int maxY = (int)MathF.Ceiling(pts.Max(p => p.Y));

        byte* dpix = (byte*)dSurf.pixels;
        byte* spix = (byte*)sSurf.pixels;

        void ProcessRow(int y)
        {
            if ((uint)y >= (uint)dSurf.h) return;
            byte* drow = dpix + y * dSurf.pitch;

            for (int x = minX; x < maxX; x++)
            {
                if ((uint)x >= (uint)dSurf.w) continue;

                var srcPos = Vector2.Transform(new Vector2(x + 0.5f, y + 0.5f), inv);
                int sx = (int)MathF.Floor(srcPos.X);
                int sy = (int)MathF.Floor(srcPos.Y);
                if ((uint)sx >= (uint)sSurf.w || (uint)sy >= (uint)sSurf.h) continue;

                uint* sp = (uint*)(spix + sy * sSurf.pitch + sx * 4);
                byte r, g, b, a;
                Unpack(*sp, sFmt, out r, out g, out b, out a);
                if (a == 0) continue;

                uint* dp = (uint*)(drow + x * 4);
                *dp = Pack(r, g, b, a, dFmt);
            }
        }

        int totalPixels = (maxX - minX) * (maxY - minY);
        const int MinParallelPixels = 640 * 480;
        if (totalPixels >= MinParallelPixels)
            Parallel.For(minY, maxY, ProcessRow);
        else
            for (int y = minY; y < maxY; y++) ProcessRow(y);

    UNLOCK:
        SDL.SDL_UnlockSurface(src);
        SDL.SDL_UnlockSurface(dest);
    }
}

