using System;
using System.Runtime.InteropServices;
using AbstUI.Primitives;
using AbstUI.SDL2.SDLL;
using BlingoEngine.Primitives;
using BlingoEngine.Tools;
using Xunit;

public class SdlMemberBitmapTests : IDisposable
{
    public SdlMemberBitmapTests()
    {
        Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "dummy");
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
    }

    [Fact]
    public void ConvertSurfaceToAbgrPreservesRgbaOrderForBlendInk()
    {
        nint src = SDL.SDL_CreateRGBSurfaceWithFormat(0, 1, 1, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        nint fmt = SDL.SDL_AllocFormat(SDL.SDL_PIXELFORMAT_RGBA8888);
        uint pixel = SDL.SDL_MapRGBA(fmt, 120, 64, 32, 128);
        SDL.SDL_FillRect(src, IntPtr.Zero, pixel);

        nint conv = SDL.SDL_ConvertSurfaceFormat(src, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
        var surf = Marshal.PtrToStructure<SDL.SDL_Surface>(conv);
        var bytes = new byte[4];
        Marshal.Copy(surf.pixels, bytes, 0, 4);

        // Ensure conversion produced RGBA byte order
        Assert.Equal(120, bytes[0]);
        Assert.Equal(64, bytes[1]);
        Assert.Equal(32, bytes[2]);
        Assert.Equal(128, bytes[3]);

        var result = InkPreRenderer.Apply(bytes, BlingoInkType.Blend, new AColor(0, 0, 0));
        Assert.Equal(120 * 128 / 255, result[0]);
        Assert.Equal(64 * 128 / 255, result[1]);
        Assert.Equal(32 * 128 / 255, result[2]);
        Assert.Equal(255, result[3]);

        SDL.SDL_FreeFormat(fmt);
        SDL.SDL_FreeSurface(conv);
        SDL.SDL_FreeSurface(src);
    }

    public void Dispose()
    {
        SDL.SDL_Quit();
    }
}


