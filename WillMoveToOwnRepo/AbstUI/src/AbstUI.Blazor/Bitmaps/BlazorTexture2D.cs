using AbstUI.Primitives;
using System.Runtime.InteropServices;

namespace AbstUI.Blazor.Bitmaps;

public class BlazorTexture2D : IAbstTexture2D
{
    public nint Handle { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public bool IsDisposed { get; private set; }
    public string Name { get; set; } = "";

    private readonly Dictionary<object, TextureSubscription> _users = new();

    public BlazorTexture2D(nint texture, int width, int height, string name = "")
    {
        Handle = texture;
        Width = width;
        Height = height;
        Name = name;
    }

    public IAbstUITextureUserSubscription AddUser(object user)
    {
        if (IsDisposed) throw new Exception("Texture is disposed and cannot be used anymore.");
        var sub = new TextureSubscription(this, () => RemoveUser(user));
        _users.Add(user, sub);
        return sub;
    }

    private void RemoveUser(object user)
    {
        _users.Remove(user);
        if (_users.Count == 0 && Handle != nint.Zero)
            Dispose();
    }
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        if (Handle == nint.Zero)
            return;
        //Console.WriteLine("Disposing BlazorTexture2D: " + Name);
        Blazor.Blazor_DestroyTexture(Handle);
        Handle = nint.Zero;
    }

    public nint ToSurface(nint renderer, out int w, out int h, uint? fmt = null)
    {
        if (fmt == null) fmt = Blazor.Blazor_PIXELFORMAT_ABGR8888;

        Blazor.Blazor_QueryTexture(Handle, out _, out _, out w, out h);

        // Create a render target in the desired pixel format
        var target = Blazor.Blazor_CreateTexture(renderer, fmt.Value,
            (int)Blazor.Blazor_TextureAccess.Blazor_TEXTUREACCESS_TARGET, w, h);
        if (target == nint.Zero) throw new Exception(Blazor.Blazor_GetError());

        // Save old state
        var oldTarget = Blazor.Blazor_GetRenderTarget(renderer);
        Blazor.Blazor_GetRenderDrawColor(renderer, out byte oldR, out byte oldG, out byte oldB, out byte oldA);
        Blazor.Blazor_GetTextureBlendMode(Handle, out var oldBlend);

        // Set up render target
        Blazor.Blazor_SetRenderTarget(renderer, target);
        Blazor.Blazor_SetTextureBlendMode(target, Blazor.Blazor_BlendMode.Blazor_BLENDMODE_NONE);
        Blazor.Blazor_SetRenderDrawColor(renderer, 0, 0, 0, 0); // Transparent clear
        Blazor.Blazor_RenderClear(renderer);

        // Render this texture onto the target without blending
        Blazor.Blazor_SetTextureBlendMode(Handle, Blazor.Blazor_BlendMode.Blazor_BLENDMODE_NONE);
        var rect = new Blazor.Blazor_Rect { x = 0, y = 0, w = w, h = h };
        Blazor.Blazor_RenderCopy(renderer, Handle, nint.Zero, ref rect);
        Blazor.Blazor_SetTextureBlendMode(Handle, oldBlend);

        // Read back pixels into a surface
        var surf = Blazor.Blazor_CreateRGBSurfaceWithFormat(0, w, h, 32, fmt.Value);
        var s = Marshal.PtrToStructure<Blazor.Blazor_Surface>(surf);
        Blazor.Blazor_RenderReadPixels(renderer, ref rect, fmt.Value, s.pixels, s.pitch);

        // Restore render state
        Blazor.Blazor_SetRenderTarget(renderer, oldTarget);
        Blazor.Blazor_SetRenderDrawColor(renderer, oldR, oldG, oldB, oldA);

        Blazor.Blazor_DestroyTexture(target);

        return surf; // Caller must Blazor_FreeSurface(surf)
    }



    public IAbstTexture2D Clone(nint renderer)
    {
        var cloned = CloneTexture(renderer, Handle);
        var clone = new BlazorTexture2D(cloned, Width, Height);
        return clone;
    }
    public static nint CloneTexture(nint renderer, nint src)
    {
        Blazor.Blazor_QueryTexture(src, out uint fmt, out _, out int w, out int h);

        var dst = Blazor.Blazor_CreateTexture(renderer, fmt,
            (int)Blazor.Blazor_TextureAccess.Blazor_TEXTUREACCESS_TARGET, w, h);
        if (dst == nint.Zero) throw new Exception(Blazor.Blazor_GetError());

        Blazor.Blazor_GetTextureColorMod(src, out byte r, out byte g, out byte b);
        Blazor.Blazor_GetTextureAlphaMod(src, out byte a);
        Blazor.Blazor_GetTextureBlendMode(src, out Blazor.Blazor_BlendMode blend);
        Blazor.Blazor_SetTextureColorMod(dst, r, g, b);
        Blazor.Blazor_SetTextureAlphaMod(dst, a);
        Blazor.Blazor_SetTextureBlendMode(dst, blend);

        var old = Blazor.Blazor_GetRenderTarget(renderer);
        Blazor.Blazor_SetRenderTarget(renderer, dst);
        Blazor.Blazor_RenderClear(renderer);
        var rect = new Blazor.Blazor_Rect { x = 0, y = 0, w = w, h = h };
        Blazor.Blazor_RenderCopy(renderer, src, nint.Zero, ref rect);
        Blazor.Blazor_SetRenderTarget(renderer, old);

        return dst; // caller must Blazor_DestroyTexture(dst)
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
        var fn = $"C:/temp/director/{(!string.IsNullOrWhiteSpace(folder) ? folder + "/" : "")}Blazor_{fileName}.png";
        if (File.Exists(fn)) File.Delete(fn);

        Blazor.Blazor_QueryTexture(texture, out _, out _, out int w, out int h);

        // 1) temp target with a known format
        var OUT_FMT = Blazor.Blazor_PIXELFORMAT_RGBA8888;
        var target = Blazor.Blazor_CreateTexture(renderer, OUT_FMT, (int)Blazor.Blazor_TextureAccess.Blazor_TEXTUREACCESS_TARGET, w, h);
        if (target == nint.Zero) throw new Exception(Blazor.Blazor_GetError());
        Blazor.Blazor_SetTextureBlendMode(target, Blazor.Blazor_BlendMode.Blazor_BLENDMODE_NONE);
        Blazor.Blazor_SetRenderDrawColor(renderer, 0, 0, 0, 0);   // transparent clear
        Blazor.Blazor_RenderClear(renderer);

        // 2) render texture -> target
        Blazor.Blazor_GetTextureBlendMode(texture, out var oldBlend);
        Blazor.Blazor_SetTextureBlendMode(texture, Blazor.Blazor_BlendMode.Blazor_BLENDMODE_NONE);
        var old = Blazor.Blazor_GetRenderTarget(renderer);
        Blazor.Blazor_SetRenderTarget(renderer, target);
        Blazor.Blazor_RenderClear(renderer);
        var rect = new Blazor.Blazor_Rect { x = 0, y = 0, w = w, h = h };
        Blazor.Blazor_RenderCopy(renderer, texture, nint.Zero, ref rect);
        Blazor.Blazor_SetTextureBlendMode(texture, oldBlend);     // restore

        // 3) read pixels from target
        nint surface = Blazor.Blazor_CreateRGBSurfaceWithFormat(0, w, h, 32, OUT_FMT);
        var s = Marshal.PtrToStructure<Blazor.Blazor_Surface>(surface);
        Blazor.Blazor_RenderReadPixels(renderer, ref rect, OUT_FMT, s.pixels, s.pitch);

        // 4) restore target & save
        Blazor.Blazor_SetRenderTarget(renderer, old);
        if (Blazor_image.IMG_SavePNG(surface, fn) != 0)
            throw new Exception($"IMG_SavePNG failed: {Blazor.Blazor_GetError()}");

        Blazor.Blazor_FreeSurface(surface);
        Blazor.Blazor_DestroyTexture(target);
    }

#endif
    private class TextureSubscription : IAbstUITextureUserSubscription
    {
        private readonly Action _onRelease;
        public IAbstTexture2D Texture { get; }
        public TextureSubscription(BlazorTexture2D texture, Action onRelease)
        {
            Texture = texture;
            _onRelease = onRelease;
        }

        public void Release() => _onRelease();
    }
}

