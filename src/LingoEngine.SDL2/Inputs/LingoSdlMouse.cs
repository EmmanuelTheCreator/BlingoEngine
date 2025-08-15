using AbstUI.Inputs;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;
using System.Runtime.InteropServices;

namespace LingoEngine.SDL2.Inputs;

public class LingoSdlMouse : SdlMouse<LingoMouseEvent> , ILingoFrameworkMouse
{
    private LingoMemberBitmap? _cursorImage;
    private AMouseCursor _cursor = AMouseCursor.Arrow;
    

    public LingoSdlMouse(Lazy<AbstUIMouse<LingoMouseEvent>> mouse) : base(mouse)
    {
    }
    public override void SetCursor(AMouseCursor value)
    {
        _cursor = value;
    }
    public void SetCursor(LingoMemberBitmap? image)
    {
        _cursorImage = image;
        if (image == null) return;
        image.Framework<SdlMemberBitmap>().Preload();
        var pic = image.Framework<SdlMemberBitmap>();
        if (pic.ImageData == null) return;

        var handle = GCHandle.Alloc(pic.ImageData, GCHandleType.Pinned);
        try
        {
            var rw = SDL.SDL_RWFromMem(handle.AddrOfPinnedObject(), pic.ImageData.Length);
            var surface = SDL_image.IMG_Load_RW(rw, 1);
            if (surface == nint.Zero) return;

            _sdlCursor = SDL.SDL_CreateColorCursor(surface, 0, 0);
            SDL.SDL_SetCursor(_sdlCursor);
            SDL.SDL_FreeSurface(surface);
        }
        finally
        {
            handle.Free();
        }
    }


   
}
