using System.Runtime.InteropServices;
using LingoEngine.Bitmaps;
using LingoEngine.SDL2.Inputs;
using LingoEngine.Sprites;
using LingoEngine.Tools;
using LingoEngine.Primitives;
using System.Security.Cryptography;
using AbstUI.Primitives;
using AbstUI.Tools;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Core;
using System.Threading.Tasks;

namespace LingoEngine.SDL2.Bitmaps;
public class SdlMemberBitmap : ILingoFrameworkMemberBitmap, IDisposable
{
    private LingoMemberBitmap _member = null!;
    private nint _surface = nint.Zero;
    private SDL.SDL_Surface _surfacePtr;
    private SdlTexture2D? _texture;
    private IAbstUITextureUserSubscription? _textureSubscription;
    private readonly Dictionary<LingoInkType, SdlTexture2D> _inkTextures = new();
    private readonly ISdlRootComponentContext _sdlRootContext;

    public byte[]? ImageData { get; private set; }
    public bool IsLoaded { get; private set; }
    public string Format { get; private set; } = "image/unknown";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public SDL.SDL_Surface TextureSDL => _surfacePtr;
    internal nint Surface => _surface;

    public IAbstTexture2D? TextureLingo => _texture;

    public SdlMemberBitmap(ISdlRootComponentContext sdlRootContext)
    {
        _sdlRootContext = sdlRootContext;
    }

    internal void Init(LingoMemberBitmap member)
    {
        _member = member;
    }
    public void ReleaseFromSprite(LingoSprite2D lingoSprite) { }
    public void Preload()
    {
        if (IsLoaded)
            return;
        // For some unknown reason Path.Combine is not working :(
        var fullFileName = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + _member.FileName;
        if (!File.Exists(fullFileName))
            return;
        if (_textureSubscription != null)
        {
            _textureSubscription.Release();
            _textureSubscription = null;
        }
        _surface = SDL_image.IMG_Load(fullFileName);
        if (_surface == nint.Zero)
            return;

        _surfacePtr = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
        Width = _surfacePtr.w;
        Height = _surfacePtr.h;
        //var surfaceLingo = new SdlImageTexture(_surfacePtr, _surface, Width, Height);

        ImageData = File.ReadAllBytes(fullFileName);
        Format = MimeHelper.GetMimeType(_member.FileName);
        _member.Size = ImageData.Length;
        _member.Width = Width;
        _member.Height = Height;
        _texture = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), Width, Height);
        _textureSubscription = _texture.AddUser(this);
        IsLoaded = true;
    }

    public Task PreloadAsync()
    {
        Preload();
        return Task.CompletedTask;
    }

    public void Unload()
    {
        _textureSubscription?.Release();
        _textureSubscription = null;
        ClearCache();
        if (_surface != nint.Zero)
        {
            SDL.SDL_FreeSurface(_surface);
            _surface = nint.Zero;
        }

        _texture = null;
        IsLoaded = false;
    }

    public void Erase()
    {
        Unload();
        ImageData = null;
    }

    public void Dispose() { Unload(); }

    public void ImportFileInto() { }

    public IAbstTexture2D? RenderToTexture(LingoInkType ink, AColor transparentColor)
        => GetTextureForInk(ink, transparentColor, _sdlRootContext.Renderer);

    public IAbstTexture2D? GetTextureForInk(LingoInkType ink, AColor backColor, nint renderer)
    {
        if (!InkPreRenderer.CanHandle(ink) || _surface == nint.Zero)
            return null;
        var inkKey = InkPreRenderer.GetInkCacheKey(ink);


        if (_inkTextures.TryGetValue(inkKey, out var cached) && cached != null)
            return cached.Clone(_sdlRootContext.Renderer);

        nint surf = SDL.SDL_ConvertSurfaceFormat(_surface, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
        if (surf == nint.Zero)
            return null;

        var surfPtr = Marshal.PtrToStructure<SDL.SDL_Surface>(surf);
        var bytes = new byte[surfPtr.w * surfPtr.h * 4];
        Marshal.Copy(surfPtr.pixels, bytes, 0, bytes.Length);
        bytes = InkPreRenderer.Apply(bytes, ink, backColor);
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            nint newSurf = SDL.SDL_CreateRGBSurfaceWithFormatFrom(handle.AddrOfPinnedObject(), surfPtr.w, surfPtr.h, 32, surfPtr.w * 4,
                SDL.SDL_PIXELFORMAT_ABGR8888);
            var tex = SDL.SDL_CreateTextureFromSurface(renderer, newSurf);
            SDL.SDL_FreeSurface(newSurf);
            SDL.SDL_FreeSurface(surf);
            //var texture = new SdlTexture2D(SDL.SDL_CreateTextureFromSurface(_sdlRootContext.Renderer, _surface), Width, Height);
            var texture = new SdlTexture2D(tex, surfPtr.w, surfPtr.h);
            _inkTextures[inkKey] = texture;

            //SdlTexture2D.DebugToDisk(_sdlRootContext.Renderer, texture.Handle,"Members", _member.Name);
            return texture.Clone(_sdlRootContext.Renderer);
        }
        finally
        {
            handle.Free();
        }
    }

    public bool IsPixelTransparent(int x, int y)
    {
        if (_surface == nint.Zero)
            return false;

        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return true;

        SDL.SDL_LockSurface(_surface);
        try
        {
            var surf = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
            var format = Marshal.PtrToStructure<SDL.SDL_PixelFormat>(surf.format);
            int bpp = format.BytesPerPixel;
            int offset = y * surf.pitch + x * bpp;

            uint pixel = bpp switch
            {
                1 => Marshal.ReadByte(surf.pixels, offset),
                2 => (uint)Marshal.ReadInt16(surf.pixels, offset),
                3 => (uint)(Marshal.ReadByte(surf.pixels, offset) |
                              Marshal.ReadByte(surf.pixels, offset + 1) << 8 |
                              Marshal.ReadByte(surf.pixels, offset + 2) << 16),
                _ => (uint)Marshal.ReadInt32(surf.pixels, offset)
            };

            SDL.SDL_GetRGBA(pixel, surf.format, out _, out _, out _, out byte a);
            return a == 0;
        }
        finally
        {
            SDL.SDL_UnlockSurface(_surface);
        }
    }

    public void SetImageData(byte[] bytes) => ImageData = bytes;

    private void ClearCache()
    {
        foreach (var tex in _inkTextures.Values)
        {
            if (tex != null)
                SDL.SDL_DestroyTexture(tex.Handle);
        }
        _inkTextures.Clear();
    }



    #region Clipboard

    public void CopyToClipboard()
    {
        if (ImageData == null) return;
        var base64 = Convert.ToBase64String(ImageData);
        SdlClipboard.SetText("data:" + Format + ";base64," + base64);
    }
    public void PasteClipboardInto()
    {
        var data = SdlClipboard.GetText();
        if (string.IsNullOrEmpty(data)) return;
        var parts = data.Split(',', 2);
        if (parts.Length != 2) return;

        var bytes = Convert.FromBase64String(parts[1]);
        GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var rw = SDL.SDL_RWFromMem(pinned.AddrOfPinnedObject(), bytes.Length);
            _surface = SDL_image.IMG_Load_RW(rw, 1);
            if (_surface == nint.Zero)
            {
                Console.WriteLine("IMG_Load_RW failed: " + SDL_image.IMG_GetError());
                return;
            }

            var surf = Marshal.PtrToStructure<SDL.SDL_Surface>(_surface);
            Width = surf.w;
            Height = surf.h;
            ImageData = bytes;
            Format = parts[0].Replace("data:", "");
            _member.Size = bytes.Length;
            _member.Width = Width;
            _member.Height = Height;
            IsLoaded = true;
        }
        finally
        {
            pinned.Free();
        }


    }
    #endregion
}
