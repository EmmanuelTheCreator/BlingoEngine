using AbstUI.Primitives;
using AbstUI.Bitmaps;
using AbstUI.SDL2.SDLL;
using System.Runtime.InteropServices;
using System.IO;

namespace AbstUI.SDL2.Bitmaps;

public class SdlTexture2D : AbstBaseTexture2D<nint>
{
    private nint _renderer;
    public nint Handle { get; private set; }
    public override int Width { get; }
    public override int Height { get; }

    public SdlTexture2D(nint texture, int width, int height, string name = "", nint renderer = 0) : base(name)
    {
        Handle = texture;
        Width = width;
        Height = height;
        _renderer = renderer;
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

    public override IAbstTexture2D Clone() => Clone(_renderer);
    public IAbstTexture2D Clone(nint renderer)
    {
        var cloned = CloneTexture(renderer, Handle);
        var clone = new SdlTexture2D(cloned, Width, Height, Name + "_cloned", _renderer);
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

    public void SetRenderer(nint renderer) => _renderer = renderer;
    public override byte[] GetPixels()
    {
        if (_renderer == nint.Zero) throw new NotSupportedException("Renderer required for pixel readback.");
        return GetPixels(_renderer);
    }
    public byte[] GetPixels(nint renderer)
    {
        if (Handle == nint.Zero) return Array.Empty<byte>();

        // Use a temp RGBA8888 target + RenderReadPixels
        var OUT_FMT = SDL.SDL_PIXELFORMAT_RGBA8888;
        SDL.SDL_QueryTexture(Handle, out _, out _, out int w, out int h);

        nint prevTarget = SDL.SDL_GetRenderTarget(renderer);
        nint target = SDL.SDL_CreateTexture(renderer, OUT_FMT, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
        if (target == nint.Zero) throw new Exception(SDL.SDL_GetError());
        SDL.SDL_SetTextureBlendMode(target, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);

        SDL.SDL_SetRenderTarget(renderer, target);
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        SDL.SDL_RenderClear(renderer);

        // draw source texture onto target
        SDL.SDL_GetTextureBlendMode(Handle, out var oldBlend);
        SDL.SDL_SetTextureBlendMode(Handle, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);
        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
        SDL.SDL_RenderCopy(renderer, Handle, IntPtr.Zero, ref rect);
        SDL.SDL_SetTextureBlendMode(Handle, oldBlend);

        // readback
        int pitch = w * 4; // RGBA8888
        byte[] pixels = new byte[pitch * h];
        unsafe
        {
            fixed (byte* p = pixels)
            {
                if (SDL.SDL_RenderReadPixels(renderer, ref rect, OUT_FMT, (IntPtr)p, pitch) != 0)
                {
                    SDL.SDL_SetRenderTarget(renderer, prevTarget);
                    SDL.SDL_DestroyTexture(target);
                    throw new Exception(SDL.SDL_GetError());
                }
            }
        }

        SDL.SDL_SetRenderTarget(renderer, prevTarget);
        SDL.SDL_DestroyTexture(target);
        return pixels;
    }
    public override void SetARGBPixels(byte[] argbPixels)
    {
        if (Handle == nint.Zero) return;
        if (argbPixels == null || argbPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected ARGB8888 buffer with Width*Height*4 bytes.", nameof(argbPixels));

        UploadPixelsFrom(argbPixels, SDL.SDL_PIXELFORMAT_ARGB8888);
        //DebugWriteToDiskInc(_renderer);
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        if (Handle == nint.Zero) return;
        if (rgbaPixels == null || rgbaPixels.Length != Width * Height * 4)
            throw new ArgumentException("Expected RGBA8888 buffer with Width*Height*4 bytes.", nameof(rgbaPixels));

        UploadPixelsFrom(rgbaPixels, SDL.SDL_PIXELFORMAT_RGBA8888);
        //DebugWriteToDiskInc(_renderer);
    }

    // SdlTexture2D
    private void UploadPixelsFrom(byte[] srcPixels, uint srcFormat)
    {
        SDL.SDL_QueryTexture(Handle, out uint dstFmt, out int access, out int w, out int h);
        if (w != Width || h != Height) throw new InvalidOperationException("Texture size mismatch.");

        var gch = GCHandle.Alloc(srcPixels, GCHandleType.Pinned);
        try
        {
            nint src = SDL.SDL_CreateRGBSurfaceWithFormatFrom(
                gch.AddrOfPinnedObject(), Width, Height, 32, Width * 4, srcFormat);
            if (src == nint.Zero) throw new Exception(SDL.SDL_GetError());

            nint conv = (dstFmt == srcFormat) ? src : SDL.SDL_ConvertSurfaceFormat(src, dstFmt, 0);
            if (conv == nint.Zero) { SDL.SDL_FreeSurface(src); throw new Exception(SDL.SDL_GetError()); }
            var s = Marshal.PtrToStructure<SDL.SDL_Surface>(conv);

            if (access == (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING)
            {
                if (SDL.SDL_LockTexture(Handle, IntPtr.Zero, out IntPtr dst, out int pitch) != 0)
                { if (conv != src) SDL.SDL_FreeSurface(conv); SDL.SDL_FreeSurface(src); throw new Exception(SDL.SDL_GetError()); }
                try
                {
                    unsafe
                    {
                        byte* d = (byte*)dst;
                        byte* sp = (byte*)s.pixels;
                        int rowBytes = Math.Min(s.pitch, pitch);
                        for (int y = 0; y < Height; y++)
                            Buffer.MemoryCopy(sp + y * s.pitch, d + y * pitch, pitch, rowBytes);
                    }
                }
                finally { SDL.SDL_UnlockTexture(Handle); }
            }
            else if (access == (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC)
            {
                if (SDL.SDL_UpdateTexture(Handle, IntPtr.Zero, s.pixels, s.pitch) != 0)
                { if (conv != src) SDL.SDL_FreeSurface(conv); SDL.SDL_FreeSurface(src); throw new Exception(SDL.SDL_GetError()); }
            }
            else // TARGET: upload via renderer
            {
                if (_renderer == nint.Zero) { if (conv != src) SDL.SDL_FreeSurface(conv); SDL.SDL_FreeSurface(src); throw new NotSupportedException("Renderer required for target upload."); }

                nint staging = SDL.SDL_CreateTexture(_renderer, dstFmt, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, w, h);
                if (staging == nint.Zero) { if (conv != src) SDL.SDL_FreeSurface(conv); SDL.SDL_FreeSurface(src); throw new Exception(SDL.SDL_GetError()); }

                if (SDL.SDL_UpdateTexture(staging, IntPtr.Zero, s.pixels, s.pitch) != 0)
                { SDL.SDL_DestroyTexture(staging); if (conv != src) SDL.SDL_FreeSurface(conv); SDL.SDL_FreeSurface(src); throw new Exception(SDL.SDL_GetError()); }

                SDL.SDL_GetTextureBlendMode(staging, out var oldBlendStaging);
                SDL.SDL_SetTextureBlendMode(staging, SDL.SDL_BlendMode.SDL_BLENDMODE_NONE);

                var oldTarget = SDL.SDL_GetRenderTarget(_renderer);
                SDL.SDL_SetRenderTarget(_renderer, Handle);
                var rect = new SDL.SDL_Rect { x = 0, y = 0, w = w, h = h };
                SDL.SDL_RenderCopy(_renderer, staging, IntPtr.Zero, ref rect);
                SDL.SDL_SetRenderTarget(_renderer, oldTarget);

                SDL.SDL_SetTextureBlendMode(staging, oldBlendStaging);
                SDL.SDL_DestroyTexture(staging);
            }

            if (conv != src) SDL.SDL_FreeSurface(conv);
            SDL.SDL_FreeSurface(src);
        }
        finally { gch.Free(); }
    }






#if DEBUG
    private static int _incrementerDebug = 0;

    public static void ResetDebuggerInc() => _incrementerDebug = 0;
    public void DebugWriteToDiskInc(nint renderer)
    {
        _incrementerDebug++;
        DebugToDisk(renderer, Handle, $"{Name}_{_incrementerDebug}");
    }

    public void DebugWriteToDisk(nint renderer)
        => DebugToDisk(renderer, Handle, Name);
    public static void DebugToDisk(nint renderer, nint texture, string fileName)
        => DebugToDisk(renderer, texture, "", fileName);
    public static void DebugToDisk(nint renderer, nint texture, string folder, string fileName)
    {
        if (texture == nint.Zero)
            throw new Exception("DebugToDisk: texture is null.");

        var baseDir = Path.Combine(Path.GetTempPath(), "director");
        Directory.CreateDirectory(baseDir);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            baseDir = Path.Combine(baseDir, folder);
            Directory.CreateDirectory(baseDir);
        }
        var fn = Path.Combine(baseDir, $"SDL_{fileName}.png");
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
