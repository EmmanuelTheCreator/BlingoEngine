using System.Numerics;
using System.Runtime.InteropServices;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.FilmLoops;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Inputs;
using LingoEngine.SDL2.SDLL;
using LingoEngine.Sprites;

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
    public ILingoTexture2D? TextureLingo { get; private set; }
    public APoint Offset { get; private set; }
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
        if (TextureLingo is SdlTexture2D tex && tex.Handle != nint.Zero)
        {
            SDL.SDL_DestroyTexture(tex.Handle);
            TextureLingo = null;
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
    public ILingoTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor) => TextureLingo;
    /// <summary>
    /// Composes the provided layers into a single SDL texture using the
    /// <see cref="LingoFilmLoopComposer"/> for layout and transforms.
    /// </summary>
    /// <returns>The composed texture for rendering.</returns>
    public ILingoTexture2D ComposeTexture(ILingoSprite2DLight hostSprite, IReadOnlyList<LingoSprite2DVirtual> layers, int frame)
    {
        //return new SdlTexture2D(nint.Zero, 0, 0);
        //var sdlSprite = hostSprite.FrameworkObj as SdlSprite;
        //if (sdlSprite == null)
        //    return Texture ?? new SdlTexture2D(nint.Zero, 0, 0);
        
        var prep = LingoFilmLoopComposer.Prepare(_member, Framing, layers);
        Offset = prep.Offset;
        int width = prep.Width;
        int height = prep.Height;

        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        if (surface == nint.Zero)
            return TextureLingo ?? new SdlTexture2D(nint.Zero, width, height);
        var i = 0;
        foreach (var info in prep.Layers)
        {
            i++;
            nint srcImg = nint.Zero;
            SdlTexture2D? textureSdl = null;
            int widthS = 0;
            int heightS = 0;
            if (info.Sprite2D.Member is LingoMemberBitmap pic && pic.FrameworkObj is SdlMemberBitmap bmp)
            {
                textureSdl = ((ILingoFrameworkMemberBitmap)bmp).TextureLingo as SdlTexture2D;
                if (textureSdl == null) continue;
                textureSdl = bmp.GetTextureForInk(info.Ink, info.BackColor, _sdlRootContext.Renderer) as SdlTexture2D;
                if (textureSdl == null) continue;
                //DebugToDisk(textureSdl.Handle, $"filmloop_{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
                srcImg = textureSdl.Handle;
                widthS = textureSdl.Width;
                heightS = textureSdl.Height;
            }
            else
            {
                if (info.Sprite2D.Member != null)
                    info.Sprite2D.Member.Preload();
                if (info.Sprite2D.Texture == null) continue;

                textureSdl = (SdlTexture2D)info.Sprite2D.Texture;

                //DebugToDisk(textureSdl.Handle, $"filmloop_{frame}/{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
                srcImg = textureSdl.Handle;
                widthS = textureSdl.Width;
                heightS = textureSdl.Height;
            }
            if (srcImg == nint.Zero)
                continue;
            //DebugToDisk(srcImg, $"filmloop_{i}_{info.Sprite2D.SpriteNum}_{info.Sprite2D.Member?.Name}");
            nint srcSurf = textureSdl.ToSurface(_sdlRootContext.Renderer, out widthS, out heightS);
            bool freeSrc = true;
            var sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(srcSurf);
            var pixFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sSurf.format).format;
            if (pixFmt != SDL.SDL_PIXELFORMAT_ABGR8888)
            {
                nint conv = SDL.SDL_ConvertSurfaceFormat(srcSurf, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
                if (conv == nint.Zero)
                    continue;
                srcSurf = conv;
                sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(srcSurf);
                widthS = sSurf.w;
                heightS = sSurf.h;
                freeSrc = true;
            }

            if (info.SrcX != 0 || info.SrcY != 0 || info.SrcW != widthS || info.SrcH != heightS || info.DestW != info.SrcW || info.DestH != info.SrcH)
            {
                nint cropped = SDL.SDL_CreateRGBSurfaceWithFormat(0, info.DestW, info.DestH, 32, SDL.SDL_PIXELFORMAT_ABGR8888);
                if (cropped == nint.Zero)
                {
                    if (freeSrc)
                        SDL.SDL_FreeSurface(srcSurf);
                    continue;
                }
                SDL.SDL_Rect srect = new SDL.SDL_Rect { x = info.SrcX, y = info.SrcY, w = info.SrcW, h = info.SrcH };
                SDL.SDL_Rect drect = new SDL.SDL_Rect { x = 0, y = 0, w = info.DestW, h = info.DestH };
                if (Framing == LingoFilmLoopFraming.Scale)
                    SDL.SDL_BlitScaled(srcSurf, ref srect, cropped, ref drect);
                else
                    SDL.SDL_BlitSurface(srcSurf, ref srect, cropped, ref drect);
                if (freeSrc)
                    SDL.SDL_FreeSurface(srcSurf);
                srcSurf = cropped;
                freeSrc = true;
            }

            BlendSurface(surface, srcSurf, info.Transform.Matrix, info.Alpha);

            if (freeSrc)
                SDL.SDL_FreeSurface(srcSurf);
        }

        nint texture = SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, surface);
        //DebugToDisk(texture, $"filmloop_{frame}_{_member.Name}_{hostSprite.Name}");
        SDL.SDL_FreeSurface(surface);
        TextureLingo = new SdlTexture2D(texture, width, height, $"Filmloop_{frame}_Member_" + _member.Name);
        return TextureLingo;
    }
#if DEBUG
    public void DebugToDisk(nint texture, string filName) => SdlTexture2D.DebugToDisk(_sdlRootContext.Renderer, texture, filName);
#endif
    /// <summary>
    /// Blends <paramref name="src"/> onto <paramref name="dest"/> using an affine transform.
    /// </summary>
    private static unsafe void BlendSurface(nint dest, nint src, Matrix3x2 transform, float _)
    {
        SDL.SDL_LockSurface(dest);
        SDL.SDL_LockSurface(src);

        var dSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(dest);
        var sSurf = Marshal.PtrToStructure<SDL.SDL_Surface>(src);
        var dFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(dSurf.format);
        var sFmt = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sSurf.format);

        if (dSurf.format == IntPtr.Zero || sSurf.format == IntPtr.Zero) goto UNLOCK;
        if (dFmt.BytesPerPixel != 4 || sFmt.BytesPerPixel != 4) goto UNLOCK;

        static void Unpack(uint px, in SDL.SDL_PixelFormat f, out byte r, out byte g, out byte b, out byte a)
        {
            r = (byte)((px & f.Rmask) >> f.Rshift);
            g = (byte)((px & f.Gmask) >> f.Gshift);
            b = (byte)((px & f.Bmask) >> f.Bshift);
            a = (byte)((px & f.Amask) >> f.Ashift);
            // normalize to 8-bit if losses exist
            r = (byte)(r * 255 / (f.Rmask >> f.Rshift));
            g = (byte)(g * 255 / (f.Gmask >> f.Gshift));
            b = (byte)(b * 255 / (f.Bmask >> f.Bshift));
            a = (byte)(a * 255 / (f.Amask >> f.Ashift));
        }
        static uint Pack(byte r, byte g, byte b, byte a, in SDL.SDL_PixelFormat f)
        {
            uint R = (uint)(r * (f.Rmask >> f.Rshift) / 255) << f.Rshift;
            uint G = (uint)(g * (f.Gmask >> f.Gshift) / 255) << f.Gshift;
            uint B = (uint)(b * (f.Bmask >> f.Bshift) / 255) << f.Bshift;
            uint A = (uint)(a * (f.Amask >> f.Ashift) / 255) << f.Ashift;
            return (R & f.Rmask) | (G & f.Gmask) | (B & f.Bmask) | (A & f.Amask);
        }

        Matrix3x2.Invert(transform, out var inv);

        Vector2[] pts = {
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
                if (a == 0) continue; // skip fully transparent

                uint* dp = (uint*)(drow + x * 4);
                *dp = Pack(r, g, b, a, dFmt); // no extra blending
            }
        });

        UNLOCK:
        SDL.SDL_UnlockSurface(src);
        SDL.SDL_UnlockSurface(dest);
    }



    public void Dispose() { Unload(); }

    
}
