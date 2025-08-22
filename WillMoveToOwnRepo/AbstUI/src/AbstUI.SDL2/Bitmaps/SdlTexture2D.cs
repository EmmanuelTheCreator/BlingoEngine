using AbstUI.Primitives;
using AbstUI.Bitmaps;
using AbstUI.SDL2.SDLL;
using System.Runtime.InteropServices;

namespace AbstUI.SDL2.Bitmaps;

public class SdlTexture2D : AbstBaseTexture2D<nint>
{
    public nint Handle { get; private set; }
    public override int Width { get; }
    public override int Height { get; }

    public SdlTexture2D(nint texture, int width, int height, string name = "") : base(name)
    {
        Handle = texture;
        Width = width;
        Height = height;
    }

    protected override void DisposeTexture()
    {
        if (Handle == nint.Zero)
            return;
        SDL.SDL_DestroyTexture(Handle);
        Handle = nint.Zero;
    }

    public nint ToSurface(nint renderer, out int w, out int h, uint? fmt = null)
    {
        if (fmt == null) fmt = SDL.SDL_PIXELFORMAT_ABGR8888;

        SDL.SDL_QueryTexture(Handle, out _, out _, out w, out h);

        // Create a render target in the desired pixel format
        var target = SDL.SDL_CreateTexture(renderer, fmt.Value,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
        if (target == nint.Zero) throw new Exception(SDL.SDL_GetError());

        // Save old state
        var oldTarget = SDL.SDL_GetRenderTarget(renderer);
        SDL.SDL_GetRenderDrawColor(renderer, out byte oldR, out byte oldG, out byte oldB, out byte oldA);
        SDL.SDL_GetTextureBlendMode(Handle, out var oldBlend);

        // Set up render target
        SDL.SDL_SetRenderTarget(renderer, target);
        SDL.SDL_SetTextureBlendMode(target, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        SDL.SDL_RenderClear(renderer);

        // Render this texture onto the target without blending
        SDL.SDL_SetTextureBlendMode(Handle, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);
        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
        SDL.SDL_RenderCopy(renderer, Handle, nint.Zero, ref rect);
        SDL.SDL_SetTextureBlendMode(Handle, oldBlend);

        // Read back pixels into a surface
        var surf = SDL.SDL_CreateRGBSurfaceWithFormat(0, w, h, 32, fmt.Value);
        var s = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
        SDL.SDL_RenderReadPixels(renderer, ref rect, fmt.Value, s.pixels, s.pitch);

        // Restore render state
        SDL.SDL_SetRenderTarget(renderer, oldTarget);
        SDL.SDL_SetRenderDrawColor(renderer, oldR, oldG, oldB, oldA);

        SDL.SDL_DestroyTexture(target);

        return surf; // Caller must SDL_FreeSurface(surf)
    }

    public IAbstTexture2D Clone(nint renderer)
    {
        var cloned = CloneTexture(renderer, Handle);
        var clone = new SdlTexture2D(cloned, Width, Height);
        return clone;
    }
    public static nint CloneTexture(nint renderer, nint src)
    {
        SDL.SDL_QueryTexture(src, out uint fmt, out _, out int w, out int h);

        var dst = SDL.SDL_CreateTexture(renderer, fmt,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
        if (dst == nint.Zero) throw new Exception(SDL.SDL_GetError());

        SDL.SDL_GetTextureColorMod(src, out byte r, out byte g, out byte b);
        SDL.SDL_GetTextureAlphaMod(src, out byte a);
        SDL.SDL_GetTextureBlendMode(src, out SDL.SDL_BlendMode blend);
        SDL.SDL_SetTextureColorMod(dst, r, g, b);
        SDL.SDL_SetTextureAlphaMod(dst, a);
        SDL.SDL_SetTextureBlendMode(dst, blend);

        var old = SDL.SDL_GetRenderTarget(renderer);
        SDL.SDL_SetRenderTarget(renderer, dst);
        SDL.SDL_RenderClear(renderer);
        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
        SDL.SDL_RenderCopy(renderer, src, nint.Zero, ref rect);
        SDL.SDL_SetRenderTarget(renderer, old);

        return dst; // caller must SDL_DestroyTexture(dst)
    }

#if DEBUG
    public void DebugWriteToDisk(nint renderer)
        => DebugToDisk(renderer, Handle, Name);
    public static void DebugToDisk(nint renderer, nint texture, string fileName)
        => DebugToDisk(renderer, texture, "", fileName);
    public static void DebugToDisk(nint renderer, nint texture, string folder, string fileName)
    {
        if (texture == nint.Zero)
            throw new Exception("DebugToDisk: texture is null.");
        var fn = $"C:/temp/director/{(!string.IsNullOrWhiteSpace(folder) ? folder + "/" : "")}SDL_{fileName}.png";
        if (File.Exists(fn)) File.Delete(fn);

        SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);

        // 1) temp target with a known format
        var OUT_FMT = SDL.SDL_PIXELFORMAT_RGBA8888;
        var target = SDL.SDL_CreateTexture(renderer, OUT_FMT, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
        if (target == nint.Zero) throw new Exception(SDL.SDL_GetError());
        SDL.SDL_SetTextureBlendMode(target, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);   // transparent clear
        SDL.SDL_RenderClear(renderer);

        // 2) render texture -> target
        SDL.SDL_GetTextureBlendMode(texture, out var oldBlend);
        SDL.SDL_SetTextureBlendMode(texture, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);
        var old = SDL.SDL_GetRenderTarget(renderer);
        SDL.SDL_SetRenderTarget(renderer, target);
        SDL.SDL_RenderClear(renderer);
        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
        SDL.SDL_RenderCopy(renderer, texture, nint.Zero, ref rect);
        SDL.SDL_SetTextureBlendMode(texture, oldBlend);     // restore

        // 3) read pixels from target
        nint surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, w, h, 32, OUT_FMT);
        var s = Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
        SDL.SDL_RenderReadPixels(renderer, ref rect, OUT_FMT, s.pixels, s.pitch);

        // 4) restore target & save
        SDL.SDL_SetRenderTarget(renderer, old);
        if (SDL_image.IMG_SavePNG(surface, fn) != 0)
            throw new Exception($"IMG_SavePNG failed: {SDL.SDL_GetError()}");

        SDL.SDL_FreeSurface(surface);
        SDL.SDL_DestroyTexture(target);
    }
#endif
}
