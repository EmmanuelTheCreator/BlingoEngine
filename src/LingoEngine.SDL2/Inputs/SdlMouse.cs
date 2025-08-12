using LingoEngine.Bitmaps;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.SDLL;
using System;
using System.Runtime.InteropServices;

namespace LingoEngine.SDL2.Inputs;

public class SdlMouse : ILingoFrameworkMouse
{
    private Lazy<LingoMouse> _lingoMouse;
    private bool _hidden;
    private LingoMemberBitmap? _cursorImage;
    private LingoMouseCursor _cursor = LingoMouseCursor.Arrow;
    private nint _sdlCursor = nint.Zero;

    public SdlMouse(Lazy<LingoMouse> mouse)
    {
        _lingoMouse = mouse;
    }

    internal void SetLingoMouse(LingoMouse mouse) => _lingoMouse = new Lazy<LingoMouse>(() => mouse);

    public void HideMouse(bool state)
    {
        _hidden = state;
        SDL.SDL_ShowCursor(state ? 0 : 1);
    }

    public void SetCursor(LingoMemberBitmap image)
    {
        _cursorImage = image;
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


    public void SetCursor(LingoMouseCursor value)
    {
        _cursor = value;
        SDL.SDL_SystemCursor sysCursor = value switch
        {
            LingoMouseCursor.Cross => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
            LingoMouseCursor.Watch => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            LingoMouseCursor.IBeam => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
            LingoMouseCursor.SizeAll => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
            LingoMouseCursor.SizeNWSE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE,
            LingoMouseCursor.SizeNESW => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW,
            LingoMouseCursor.SizeWE => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
            LingoMouseCursor.SizeNS => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
            LingoMouseCursor.UpArrow => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            LingoMouseCursor.Blank => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            LingoMouseCursor.Finger => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
            LingoMouseCursor.Drag => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
            LingoMouseCursor.Help => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
            LingoMouseCursor.Wait => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
            LingoMouseCursor.NotAllowed => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO,
            _ => SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW
        };

        if (_sdlCursor != nint.Zero)
            SDL.SDL_FreeCursor(_sdlCursor);

        _sdlCursor = SDL.SDL_CreateSystemCursor(sysCursor);
        SDL.SDL_SetCursor(_sdlCursor);
    }

    public void Release()
    {
        if (_sdlCursor != nint.Zero)
        {
            SDL.SDL_FreeCursor(_sdlCursor);
            _sdlCursor = nint.Zero;
        }
    }

    public void ReplaceMouseObj(LingoMouse lingoMouse)
    {
        _lingoMouse = new Lazy<LingoMouse>(() => lingoMouse);
    }

    public void ProcessEvent(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                _lingoMouse.Value.MouseH = e.motion.x;
                _lingoMouse.Value.MouseV = e.motion.y;
                _lingoMouse.Value.DoMouseMove();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = true;
                    _lingoMouse.Value.LeftMouseDown = true;
                    _lingoMouse.Value.DoubleClick = e.button.clicks > 1;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = true;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = true;
                }
                _lingoMouse.Value.DoMouseDown();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                _lingoMouse.Value.MouseH = e.button.x;
                _lingoMouse.Value.MouseV = e.button.y;
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    _lingoMouse.Value.MouseDown = false;
                    _lingoMouse.Value.LeftMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_RIGHT)
                {
                    _lingoMouse.Value.RightMouseDown = false;
                }
                else if (e.button.button == SDL.SDL_BUTTON_MIDDLE)
                {
                    _lingoMouse.Value.MiddleMouseDown = false;
                }
                _lingoMouse.Value.DoMouseUp();
                break;
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                SDL.SDL_GetMouseState(out var x, out var y);
                _lingoMouse.Value.MouseH = x;
                _lingoMouse.Value.MouseV = y;
                _lingoMouse.Value.DoMouseWheel(e.wheel.y);
                break;
        }
    }

    ~SdlMouse()
    {
        if (_sdlCursor != nint.Zero)
            SDL.SDL_FreeCursor(_sdlCursor);
    }
}
