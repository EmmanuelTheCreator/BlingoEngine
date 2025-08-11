using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Sprites;
using LingoEngine.Sprites;

namespace LingoEngine.SDL2.Pictures;

public class SdlMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
{
    private LingoFilmLoopMember _member = null!;
    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public ILingoTexture2D? Texture { get; private set; }
    public LingoPoint Offset { get; private set; }
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;

    internal void Init(LingoFilmLoopMember member)
    {
        _member = member;
    }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void Preload()
    {
        IsLoaded = true;
    }

    public void Unload()
    {
        if (Texture is SdlTexture2D tex && tex.Texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Texture);
            Texture = null;
        }
        IsLoaded = false;
    }

    public void Erase()
    {
        Media = null;
        Unload();
    }

    public void ImportFileInto()
    {
        // not implemented
    }

    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        SdlClipboard.SetText(base64);
    }

    public void PasteClipboardInto()
    {
        var data = SdlClipboard.GetText();
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            Media = Convert.FromBase64String(data);
        }
        catch
        {
        }
    }

    public ILingoTexture2D ComposeTexture(LingoSprite2D hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers)
    {
        var sdlSprite = hostSprite.FrameworkObj as SdlSprite;
        if (sdlSprite == null)
            return Texture ?? new SdlTexture2D(nint.Zero, 0, 0);

        var bounds = _member.GetBoundingBox();
        Offset = new LingoPoint(-bounds.Left, -bounds.Top);
        int width = (int)MathF.Ceiling(bounds.Width);
        int height = (int)MathF.Ceiling(bounds.Height);

        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        if (surface == nint.Zero)
            return Texture ?? new SdlTexture2D(nint.Zero, width, height);

        foreach (var layer in layers)
        {
            if (layer.Member is not LingoMemberBitmap pic)
                continue;
            var bmp = pic.Framework<SdlMemberBitmap>();
            var src = bmp.Surface;
            if (src == nint.Zero)
                continue;

            int destW = (int)layer.Width;
            int destH = (int)layer.Height;
            int srcW = bmp.Width;
            int srcH = bmp.Height;

            nint srcSurf = src;
            bool freeSrc = false;
            if (Framing == LingoFilmLoopFraming.Scale)
            {
                if (destW != srcW || destH != srcH)
                {
                    srcSurf = SDL.SDL_CreateRGBSurfaceWithFormat(0, destW, destH, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
                    if (srcSurf == nint.Zero)
                        continue;
                    SDL.SDL_Rect srect = new SDL.SDL_Rect { x = 0, y = 0, w = srcW, h = srcH };
                    SDL.SDL_Rect drect = new SDL.SDL_Rect { x = 0, y = 0, w = destW, h = destH };
                    SDL.SDL_BlitScaled(src, ref srect, srcSurf, ref drect);
                    freeSrc = true;
                }
            }
            else
            {
                int cropW = Math.Min(destW, srcW);
                int cropH = Math.Min(destH, srcH);
                int cropX = (srcW - cropW) / 2;
                int cropY = (srcH - cropH) / 2;
                srcSurf = SDL.SDL_CreateRGBSurfaceWithFormat(0, cropW, cropH, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
                if (srcSurf == nint.Zero)
                    continue;
                SDL.SDL_Rect srect = new SDL.SDL_Rect { x = cropX, y = cropY, w = cropW, h = cropH };
                SDL.SDL_Rect drect = new SDL.SDL_Rect { x = 0, y = 0, w = cropW, h = cropH };
                SDL.SDL_BlitSurface(src, ref srect, srcSurf, ref drect);
                destW = cropW;
                destH = cropH;
                freeSrc = true;
            }

            var srcCenter = new Vector2(destW / 2f, destH / 2f);
            var pos = new Vector2(layer.LocH + Offset.X, layer.LocV + Offset.Y);
            var scale = new Vector2(layer.FlipH ? -1 : 1, layer.FlipV ? -1 : 1);
            float skewX = (float)Math.Tan(layer.Skew * Math.PI / 180f);
            var transform = Matrix3x2.Identity;
            transform *= Matrix3x2.CreateTranslation(-srcCenter);
            transform *= Matrix3x2.CreateScale(scale);
            transform *= Matrix3x2.CreateSkew(skewX, 0);
            transform *= Matrix3x2.CreateRotation((float)(layer.Rotation * Math.PI / 180f));
            transform *= Matrix3x2.CreateTranslation(pos);

            BlendSurface(surface, srcSurf, transform, Math.Clamp(layer.Blend / 100f, 0f, 1f));

            if (freeSrc)
                SDL.SDL_FreeSurface(srcSurf);
        }

        if (Texture is SdlTexture2D oldTex && oldTex.Texture != nint.Zero)
            SDL.SDL_DestroyTexture(oldTex.Texture);

        nint texture = SDL.SDL_CreateTextureFromSurface(sdlSprite.Renderer, surface);
        SDL.SDL_FreeSurface(surface);
        Texture = new SdlTexture2D(texture, width, height);
        return Texture;
    }

    /// <summary>
    /// Blends <paramref name="src"/> onto <paramref name="dest"/> using an affine transform.
    /// TODO: share logic with Godot implementation and investigate caching small frames.
    /// </summary>
    private static unsafe void BlendSurface(nint dest, nint src, Matrix3x2 transform, float alpha)
    {
        SDL.SDL_LockSurface(dest);
        SDL.SDL_LockSurface(src);
        var dSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(dest);
        var sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(src);

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

        for (int y = minY; y < maxY; y++)
        {
            if (y < 0 || y >= dSurf.h) continue;
            for (int x = minX; x < maxX; x++)
            {
                if (x < 0 || x >= dSurf.w) continue;
                var srcPos = Vector2.Transform(new Vector2(x + 0.5f, y + 0.5f), inv);
                int sx = (int)MathF.Floor(srcPos.X);
                int sy = (int)MathF.Floor(srcPos.Y);
                if (sx < 0 || sy < 0 || sx >= sSurf.w || sy >= sSurf.h)
                    continue;
                byte* sp = spix + sy * sSurf.pitch + sx * 4;
                float a = sp[3] / 255f * alpha;
                if (a <= 0f) continue;
                byte* dp = dpix + y * dSurf.pitch + x * 4;
                float invA = 1f - a;
                dp[0] = (byte)(sp[0] * a + dp[0] * invA);
                dp[1] = (byte)(sp[1] * a + dp[1] * invA);
                dp[2] = (byte)(sp[2] * a + dp[2] * invA);
                dp[3] = (byte)(sp[3] * a + dp[3] * invA);
            }
        }

        SDL.SDL_UnlockSurface(src);
        SDL.SDL_UnlockSurface(dest);
    }

    public void Dispose() { Unload(); }
}
