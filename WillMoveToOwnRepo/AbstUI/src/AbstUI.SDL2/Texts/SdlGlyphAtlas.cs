using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Texts;

internal sealed class SdlGlyphAtlas : IDisposable
{
    private readonly nint _renderer;
    private readonly nint _font;
    private readonly int _width;
    private readonly int _height;
    private readonly nint _surface;
    private readonly nint _texture;

    private int _nextX;
    private int _nextY;
    private int _rowHeight;

    private struct Glyph
    {
        public SDL.SDL_Rect Rect;
        public int Advance;
        public int MinX;
        public int MaxY;
    }

    private readonly Dictionary<int, Glyph> _glyphs = new();

    public SdlGlyphAtlas(nint renderer, nint font, int width = 1024, int height = 1024)
    {
        _renderer = renderer;
        _font = font;
        _width = width;
        _height = height;
        _surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_RGBA8888);
        _texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, width, height);
        SDL.SDL_SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public void EnsureGlyph(int codepoint)
    {
        if (_glyphs.ContainsKey(codepoint))
            return;

        SDL.SDL_Color white = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
        nint glyphSurface = SDL_ttf.TTF_RenderGlyph32_Blended(_font, (uint)codepoint, white);
        if (glyphSurface == nint.Zero)
            return;

        SDL.SDL_Surface surf = Marshal.PtrToStructure<SDL.SDL_Surface>(glyphSurface);
        int w = surf.w;
        int h = surf.h;

        if (_nextX + w >= _width)
        {
            _nextX = 0;
            _nextY += _rowHeight;
            _rowHeight = 0;
        }

        if (_nextY + h >= _height)
        {
            SDL.SDL_FreeSurface(glyphSurface);
            return; // atlas full
        }

        SDL.SDL_Rect dest = new SDL.SDL_Rect { x = _nextX, y = _nextY, w = w, h = h };
        SDL.SDL_BlitSurface(glyphSurface, IntPtr.Zero, _surface, ref dest);
        SDL.SDL_UpdateTexture(_texture, ref dest, surf.pixels, surf.pitch);
        SDL.SDL_FreeSurface(glyphSurface);

        SDL_ttf.TTF_GlyphMetrics32(_font, (uint)codepoint, out int minx, out _, out _, out int maxy, out int advance);

        _glyphs[codepoint] = new Glyph
        {
            Rect = dest,
            Advance = advance,
            MinX = minx,
            MaxY = maxy
        };

        _nextX += w;
        _rowHeight = Math.Max(_rowHeight, h);
    }

    public void DrawRun(ReadOnlySpan<int> run, int x, int baselineY, SDL.SDL_Color color)
    {
        SDL.SDL_SetTextureColorMod(_texture, color.r, color.g, color.b);
        SDL.SDL_SetTextureAlphaMod(_texture, color.a);

        foreach (var cp in run)
        {
            EnsureGlyph(cp);
            if (!_glyphs.TryGetValue(cp, out var g))
                continue;

            SDL.SDL_Rect src = g.Rect;
            SDL.SDL_Rect dst = new SDL.SDL_Rect
            {
                x = x + g.MinX,
                y = baselineY - g.MaxY,
                w = src.w,
                h = src.h
            };
            SDL.SDL_RenderCopy(_renderer, _texture, ref src, ref dst);
            x += g.Advance;
        }
    }

    public int MeasureWidth(ReadOnlySpan<int> run)
    {
        int w = 0;
        foreach (var cp in run)
        {
            EnsureGlyph(cp);
            if (_glyphs.TryGetValue(cp, out var g))
                w += g.Advance;
        }
        return w;
    }

    public void Dispose()
    {
        SDL.SDL_DestroyTexture(_texture);
        SDL.SDL_FreeSurface(_surface);
    }
}

