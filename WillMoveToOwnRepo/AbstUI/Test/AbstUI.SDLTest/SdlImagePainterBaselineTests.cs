using System;
using System.IO;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.Components.Graphics;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.SDLL;
using AbstUI.Tests.Common;

namespace AbstUI.SDLTest;

public class SdlImagePainterBaselineTests
{
    [Fact]
    public void DescenderGlyphExtendsBelowBaseline()
    {
        SDL.SDL_SetHint("SDL_RENDER_DRIVER", "software");
        SDL.SDL_SetHint("SDL_VIDEODRIVER", "dummy");
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            throw new Exception(SDL.SDL_GetError());
        if (SDL_ttf.TTF_Init() != 0)
            throw new Exception(SDL.SDL_GetError());

        var window = SDL.SDL_CreateWindow("test", 0, 0, 64, 64, SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN);
        var renderer = SDL.SDL_CreateRenderer(window, -1, 0);

        var fm = new SdlFontManager();
        fm.LoadAll();
        using var painter = new SDLImagePainter(fm, 64, 64, renderer);

        painter.DrawText(new APoint(0, 0), "h", fontSize: 32);
        var hTex = (SdlTexture2D)painter.GetTexture("h");
        var hPixels = hTex.GetPixels(renderer);
        int topH = GraphicsPixelHelper.FindTopOpaqueRow(hPixels, hTex.Width, hTex.Height);
        int bottomH = GraphicsPixelHelper.FindBottomOpaqueRow(hPixels, hTex.Width, hTex.Height);

        painter.Clear(new AColor(0, 0, 0, 0));
        painter.DrawText(new APoint(0, 0), "p", fontSize: 32);
        var pTex = (SdlTexture2D)painter.GetTexture("p");
        var pPixels = pTex.GetPixels(renderer);
        int topP = GraphicsPixelHelper.FindTopOpaqueRow(pPixels, pTex.Width, pTex.Height);
        int bottomP = GraphicsPixelHelper.FindBottomOpaqueRow(pPixels, pTex.Width, pTex.Height);

        Assert.Equal(topH, topP);
        Assert.True(bottomP >= bottomH);

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL_ttf.TTF_Quit();
        SDL.SDL_Quit();
    }
}
