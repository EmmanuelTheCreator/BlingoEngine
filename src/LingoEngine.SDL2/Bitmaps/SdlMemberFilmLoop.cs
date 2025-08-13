using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.SDLL;
using LingoEngine.SDL2.Sprites;
using LingoEngine.Sprites;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.SDL2.Pictures;

/// <summary>
/// SDL implementation of a film loop member. It composes the layers of a
/// <see cref="LingoFilmLoopMember"/> into an SDL texture for rendering.
/// </summary>
public class SdlMemberFilmLoop : ILingoFrameworkMemberFilmLoop, IDisposable
{
    ISdlRootComponentContext _sdlRootContext;
    private LingoFilmLoopMember _member = null!;
    public bool IsLoaded { get; private set; }
    public byte[]? Media { get; set; }
    public ILingoTexture2D? Texture { get; private set; }
    public LingoPoint Offset { get; private set; }
    public LingoFilmLoopFraming Framing { get; set; } = LingoFilmLoopFraming.Auto;
    public bool Loop { get; set; } = true;


    public SdlMemberFilmLoop(ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
    }

    /// <summary>Initializes the film loop with its owning member.</summary>
    internal void Init(LingoFilmLoopMember member)
    {
        _member = member;
    }

    /// <summary>SDL film loops do not retain sprites, so this is a no-op.</summary>
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }

    /// <summary>Marks the film loop as loaded.</summary>
    public void Preload()
    {
        IsLoaded = true;
    }

    /// <summary>Releases any GPU resources held by the film loop.</summary>
    public void Unload()
    {
        if (Texture is SdlTexture2D tex && tex.Texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Texture);
            Texture = null;
        }
        IsLoaded = false;
    }

    /// <summary>Clears media data and unloads any cached texture.</summary>
    public void Erase()
    {
        Media = null;
        Unload();
    }

    /// <summary>Imports media from an external file. Not implemented.</summary>
    public void ImportFileInto()
    {
        // not implemented
    }

    /// <summary>Copies the encoded media to the clipboard.</summary>
    public void CopyToClipboard()
    {
        if (Media == null) return;
        var base64 = Convert.ToBase64String(Media);
        SdlClipboard.SetText(base64);
    }

    /// <summary>Pastes encoded media from the clipboard if available.</summary>
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

    /// <summary>
    /// Composes the provided layers into a single SDL texture using the
    /// <see cref="LingoFilmLoopComposer"/> for layout and transforms.
    /// </summary>
    /// <returns>The composed texture for rendering.</returns>
    public ILingoTexture2D ComposeTexture(ILingoSprite2DLight hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers)
    {
        //var sdlSprite = hostSprite.FrameworkObj as SdlSprite;
        //if (sdlSprite == null)
        //    return Texture ?? new SdlTexture2D(nint.Zero, 0, 0);

        var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
        Offset = prep.Offset;
        int width = prep.Width;
        int height = prep.Height;

        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        if (surface == nint.Zero)
            return Texture ?? new SdlTexture2D(nint.Zero, width, height);

        foreach (var info in prep.Layers)
        {
            nint srcImg = nint.Zero;
            int widthS = 0;
            int heightS = 0;
            if (info.Sprite2D.Member is LingoMemberBitmap pic && pic.FrameworkObj is SdlMemberBitmap bmp)
            {
                var texture1 = ((ILingoFrameworkMemberBitmap)bmp).Texture as SdlImageTexture;
                if (texture1 == null) continue;
                srcImg = bmp.GetTextureForInk(info.Ink, info.BackColor, _sdlRootContext.Renderer);
                widthS= texture1.Width;
                heightS = texture1.Height;
            }
            else
            {
                if (info.Sprite2D.Texture == null) continue;
                var tex = (SdlTexture2D)info.Sprite2D.Texture;
                srcImg = tex.Texture;
                widthS = tex.Width;
                heightS = tex.Height;
            }
            if (srcImg == nint.Zero)
                continue;

            nint srcSurf = srcImg;
            bool freeSrc = false;
            if (info.SrcX != 0 || info.SrcY != 0 || info.SrcW != widthS || info.SrcH != heightS || info.DestW != info.SrcW || info.DestH != info.SrcH)
            {
                srcSurf = SDL.SDL_CreateRGBSurfaceWithFormat(0, info.DestW, info.DestH, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
                if (srcSurf == nint.Zero)
                    continue;
                SDL.SDL_Rect srect = new SDL.SDL_Rect { x = info.SrcX, y = info.SrcY, w = info.SrcW, h = info.SrcH };
                SDL.SDL_Rect drect = new SDL.SDL_Rect { x = 0, y = 0, w = info.DestW, h = info.DestH };
                if (Framing == LingoFilmLoopFraming.Scale)
                    SDL.SDL_BlitScaled(srcImg, ref srect, srcSurf, ref drect);
                else
                    SDL.SDL_BlitSurface(srcImg, ref srect, srcSurf, ref drect);
                freeSrc = true;
            }

            BlendSurface(surface, srcSurf, info.Transform.Matrix, info.Alpha);

            if (freeSrc)
                SDL.SDL_FreeSurface(srcSurf);
        }

        if (Texture is SdlTexture2D oldTex && oldTex.Texture != nint.Zero)
            SDL.SDL_DestroyTexture(oldTex.Texture);

        nint texture = SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, surface);
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

        Parallel.For(minY, maxY, y =>
        {
            if (y < 0 || y >= dSurf.h) return;
            byte* drow = dpix + y * dSurf.pitch;
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
                byte* dp = drow + x * 4;
                float invA = 1f - a;
                dp[0] = (byte)(sp[0] * a + dp[0] * invA);
                dp[1] = (byte)(sp[1] * a + dp[1] * invA);
                dp[2] = (byte)(sp[2] * a + dp[2] * invA);
                dp[3] = (byte)(sp[3] * a + dp[3] * invA);
            }
        });

        SDL.SDL_UnlockSurface(src);
        SDL.SDL_UnlockSurface(dest);
    }

    public void Dispose() { Unload(); }

   
}
