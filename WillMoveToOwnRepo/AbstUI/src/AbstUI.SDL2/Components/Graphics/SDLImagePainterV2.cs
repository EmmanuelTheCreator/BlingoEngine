using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Texts;
using System;
using System.Runtime.InteropServices;

namespace AbstUI.SDL2.Components.Graphics;

public class SDLImagePainterV2 : AbstImagePainter<nint>
{
    private readonly SdlFontManager _fontManager;
    private nint _texture;
    private nint _prevTarget;

    public SDLImagePainterV2(IAbstFontManager fontManager, int width, int height, nint renderer, bool useTextureGrid = false, int tileSize = 128)
        : base(width, height, GetMaxTexSize(renderer).W, GetMaxTexSize(renderer).H)
    {
        _fontManager = (SdlFontManager)fontManager;
        Renderer = renderer;
        UseTextureGrid = useTextureGrid;
        TileSize = tileSize;
        if (!UseTextureGrid)
        {
            _texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
        }
    }

    public nint Renderer { get; }
    protected override nint Target => _texture;
    public nint Texture => _texture;

    public static (int W, int H) GetMaxTexSize(nint renderer)
    {
        SDL.SDL_GetRendererInfo(renderer, out var info);
        return ((int)info.max_texture_width, (int)info.max_texture_height);
    }

    public override void Dispose()
    {
        DisposeTiles();
        if (_texture != nint.Zero)
        {
            SDL.SDL_DestroyTexture(_texture);
            _texture = nint.Zero;
        }
    }

    protected override void BeginRender(AColor clearColor)
    {
        _prevTarget = SDL.SDL_GetRenderTarget(Renderer);
        SDL.SDL_SetRenderTarget(Renderer, _texture);
        SDL.SDL_SetRenderDrawColor(Renderer, clearColor.R, clearColor.G, clearColor.B, clearColor.A);
        SDL.SDL_RenderClear(Renderer);
    }

    protected override void EndRender()
    {
        SDL.SDL_SetRenderTarget(Renderer, _prevTarget);
    }

    protected override void ResizeTexture(int width, int height)
    {
        if (_texture != nint.Zero)
            SDL.SDL_DestroyTexture(_texture);
        _texture = SDL.SDL_CreateTexture(Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
    }

    protected override nint CreateTileTexture(int width, int height)
    {
        return SDL.SDL_CreateTexture(Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
    }

    protected override void DestroyTileTexture(nint texture)
    {
        if (texture != nint.Zero)
            SDL.SDL_DestroyTexture(texture);
    }

    protected override void UseTexture(nint texture)
    {
        _texture = texture;
    }

    public override void SetPixel(APoint point, AColor color)
    {
        var p = point;
        var c = color;
        var resizePos = new APoint((int)p.X, (int)p.Y);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, new APoint(1, 1), _ =>
        {
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, 255);
            SDL.SDL_RenderDrawPointF(Renderer, p.X - OffsetX, p.Y - OffsetY);
        }));
        MarkDirty(resizePos, new APoint(1, 1));
    }

    public override void DrawLine(APoint start, APoint end, AColor color, float width = 1)
    {
        var s = start;
        var e = end;
        var c = color;
        int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
        int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
        var pos = new APoint(0, 0);
        var size = new APoint(maxX, maxY);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, pos, size, _ =>
        {
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, 255);
            SDL.SDL_RenderDrawLineF(Renderer, s.X - OffsetX, s.Y - OffsetY, e.X - OffsetX, e.Y - OffsetY);
        }));
        MarkDirty(pos, size);
    }

    public override void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
    {
        var r = rect;
        var c = color;
        var f = filled;
        var resizePos = new APoint((int)r.Left, (int)r.Top);
        var size = new APoint((int)r.Width, (int)r.Height);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            var rct = new SDL.SDL_Rect
            {
                x = (int)r.Left - OffsetX,
                y = (int)r.Top - OffsetY,
                w = (int)r.Width,
                h = (int)r.Height
            };
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
            if (f)
                SDL.SDL_RenderFillRect(Renderer, ref rct);
            else
                SDL.SDL_RenderDrawRect(Renderer, ref rct);
        }));
        MarkDirty(resizePos, size);
    }

    public override void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
    {
        var ctr = center;
        var rad = radius;
        var c = color;
        var f = filled;
        int r = (int)MathF.Ceiling(rad);
        var resizePos = new APoint((int)ctr.X - r, (int)ctr.Y - r);
        var size = new APoint(r * 2 + 1, r * 2 + 1);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
            for (int dy = -r; dy <= r; dy++)
            {
                float dyf = dy;
                if (MathF.Abs(dyf) > rad) continue;
                float y = ctr.Y + dyf - OffsetY;
                float dx = (float)MathF.Sqrt(MathF.Max(0, rad * rad - dyf * dyf));
                if (f)
                {
                    SDL.SDL_RenderDrawLineF(Renderer, ctr.X - dx - OffsetX, y, ctr.X + dx - OffsetX, y);
                }
                else
                {
                    SDL.SDL_RenderDrawPointF(Renderer, ctr.X - dx - OffsetX, y);
                    SDL.SDL_RenderDrawPointF(Renderer, ctr.X + dx - OffsetX, y);
                }
            }
        }));
        MarkDirty(resizePos, size);
    }

    public override void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
    {
        var ctr = center;
        var rad = radius;
        var sd = startDeg;
        var ed = endDeg;
        var segs = segments;
        var c = color;
        int r = (int)MathF.Ceiling(rad);
        var resizePos = new APoint((int)ctr.X - r, (int)ctr.Y - r);
        var size = new APoint(r * 2 + 1, r * 2 + 1);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
            float startRad = MathF.PI * sd / 180f;
            float endRad = MathF.PI * ed / 180f;
            float step = (endRad - startRad) / segs;
            float prevX = ctr.X + rad * MathF.Cos(startRad) - OffsetX;
            float prevY = ctr.Y + rad * MathF.Sin(startRad) - OffsetY;
            for (int i = 1; i <= segs; i++)
            {
                double ang = startRad + i * step;
                float x = ctr.X + (float)(rad * Math.Cos(ang)) - OffsetX;
                float y = ctr.Y + (float)(rad * Math.Sin(ang)) - OffsetY;
                SDL.SDL_RenderDrawLineF(Renderer, prevX, prevY, x, y);
                prevX = x;
                prevY = y;
            }
        }));
        MarkDirty(resizePos, size);
    }

    public override void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
    {
        if (points.Count < 2) return;
        var pts = new APoint[points.Count];
        for (int i = 0; i < points.Count; i++)
            pts[i] = points[i];
        var c = color;
        var f = filled;
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var p in pts)
        {
            if (p.X < minX) minX = (int)p.X;
            if (p.X > maxX) maxX = (int)p.X;
            if (p.Y < minY) minY = (int)p.Y;
            if (p.Y > maxY) maxY = (int)p.Y;
        }
        var pos = new APoint(minX, minY);
        var size = new APoint(maxX - minX + 1, maxY - minY + 1);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, pos, size, _ =>
        {
            SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
            for (int i = 0; i < pts.Length - 1; i++)
                SDL.SDL_RenderDrawLineF(Renderer, pts[i].X - OffsetX, pts[i].Y - OffsetY, pts[i + 1].X - OffsetX, pts[i + 1].Y - OffsetY);
            SDL.SDL_RenderDrawLineF(Renderer, pts[^1].X - OffsetX, pts[^1].Y - OffsetY, pts[0].X - OffsetX, pts[0].Y - OffsetY);
            if (f)
            {
                var p0 = pts[0];
                for (int i = 1; i < pts.Length - 1; i++)
                {
                    SDL.SDL_RenderDrawLineF(Renderer, p0.X - OffsetX, p0.Y - OffsetY, pts[i].X - OffsetX, pts[i].Y - OffsetY);
                    SDL.SDL_RenderDrawLineF(Renderer, p0.X - OffsetX, p0.Y - OffsetY, pts[i + 1].X - OffsetX, pts[i + 1].Y - OffsetY);
                }
            }
        }));
        MarkDirty(pos, size);
    }

    public override void DrawText(APoint position, string text, string? fontNamee = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var fntName = fontNamee;
        var col = color;
        var fs = fontSize;
        if (fs == 0) fs = 12;
        var w = width;
        var font = _fontManager.GetTyped(this, fntName ?? string.Empty, fs, style);
        if (font == null) return;
        var fnt = font.FontHandle;
        if (fnt == nint.Zero) { font.Release(); return; }

        string RenderLine(string line)
        {
            if (w >= 0)
            {
                while (line.Length > 0 && SDL_ttf.TTF_SizeUTF8(fnt, line, out int tw, out _) == 0 && tw > w)
                    line = line.Substring(0, line.Length - 1);
            }
            return line;
        }

        var rawLines = txt.Split('\n');
        var lines = new string[rawLines.Length];
        for (int i = 0; i < rawLines.Length; i++)
            lines[i] = RenderLine(rawLines[i]);
        int maxW = 0;
        foreach (var line in lines)
        {
            if (SDL_ttf.TTF_SizeUTF8(fnt, line, out int tw, out _) == 0 && tw > maxW)
                maxW = tw;
        }
        int lineSkip = SDL_ttf.TTF_FontLineSkip(fnt);
        int ascent = SDL_ttf.TTF_FontAscent(fnt);
        int totalH = lines.Length * lineSkip;
        int needW = (w >= 0) ? Math.Max(maxW, w) : maxW;
        var resizePos = new APoint((int)pos.X, (int)pos.Y);
        var size = new APoint(needW, totalH);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            SDL.SDL_Color c = new SDL.SDL_Color { r = col?.R ?? 0, g = col?.G ?? 0, b = col?.B ?? 0, a = 255 };

            List<(nint surf, int w, int h)> surfaces = new();
            foreach (var line in lines)
            {
                nint s = SDL_ttf.TTF_RenderUTF8_Blended(fnt, line, c);
                if (s == nint.Zero) continue;
                var sur = SDL.PtrToStructure<SDL.SDL_Surface>(s);
                surfaces.Add((s, sur.w, sur.h));
            }
            int y = (int)pos.Y - OffsetY; // was: pos.Y - ascent
            int boxW = w >= 0 ? w : Math.Max(0, Width - (int)pos.X);

            foreach (var (s, tw, th) in surfaces)
            {
                nint tex = SDL.SDL_CreateTextureFromSurface(Renderer, s);
                if (tex != nint.Zero)
                {
                    int startX = (int)pos.X;
                    if (boxW > 0)
                    {
                        switch (alignment)
                        {
                            case AbstTextAlignment.Center: startX += Math.Max(0, (boxW - tw) / 2); break;
                            case AbstTextAlignment.Right: startX += Math.Max(0, boxW - tw); break;
                        }
                    }
                    SDL.SDL_Rect dst = new SDL.SDL_Rect { x = startX - OffsetX, y = y, w = tw, h = th };
                    SDL.SDL_RenderCopy(Renderer, tex, nint.Zero, ref dst);
                    SDL.SDL_DestroyTexture(tex);
                }
                y += lineSkip;
            }
        }));
        font.Release();
        MarkDirty(resizePos, size);
    }

    public override void DrawSingleLine(APoint position, string text, string? fontName = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = default, AbstFontStyle style = AbstFontStyle.Regular)
    {
        var pos = position;
        var txt = text;
        var fntName = fontName;
        var col = color;
        var fs = fontSize;
        if (fs == 0) fs = 12;
        var w = width;
        var h = height;
        var font = _fontManager.GetTyped(this, fntName ?? string.Empty, fs, style);
        if (font == null) return;
        var fnt = font.FontHandle;
        if (fnt == nint.Zero) { font.Release(); return; }

        int ascent = SDL_ttf.TTF_FontAscent(fnt);
        int lineHeight = SDL_ttf.TTF_FontHeight(fnt);
        int calcW = w;
        int calcH = h;
        if (SDL_ttf.TTF_SizeUTF8(fnt, txt, out int tw, out _) == 0)
        {
            calcW = (w >= 0) ? Math.Max(w, tw) : tw;
            calcH = (h >= 0) ? h : lineHeight;
        }
        else
        {
            if (calcW < 0) calcW = 0;
            if (calcH < 0) calcH = lineHeight;
        }
        //return EnsureCapacity((int)pos.X + calcW, (int)(pos.Y - ascent + calcH));
        var resizePos = new APoint((int)pos.X, (int)pos.Y);
        var size = new APoint(calcW, calcH);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            SDL.SDL_Color c = new SDL.SDL_Color { r = col?.R ?? 0, g = col?.G ?? 0, b = col?.B ?? 0, a = 255 };
            nint s = SDL_ttf.TTF_RenderUTF8_Blended(fnt, txt, c);
            if (s == nint.Zero) return;
            var sur = SDL.PtrToStructure<SDL.SDL_Surface>(s);
            nint tex = SDL.SDL_CreateTextureFromSurface(Renderer, s);
            if (tex != nint.Zero)
            {
                int startX = (int)pos.X;
                if (w >= 0)
                {
                    switch (alignment)
                    {
                        case AbstTextAlignment.Center:
                            startX += Math.Max(0, (w - sur.w) / 2);
                            break;
                        case AbstTextAlignment.Right:
                            startX += Math.Max(0, w - sur.w);
                            break;
                    }
                }
                SDL.SDL_Rect dst = new SDL.SDL_Rect { x = startX - OffsetX, y = (int)pos.Y - OffsetY, w = sur.w, h = sur.h };
                SDL.SDL_RenderCopy(Renderer, tex, nint.Zero, ref dst);
                SDL.SDL_DestroyTexture(tex);
            }
            SDL.SDL_FreeSurface(s);
        }));
        font.Release();
        MarkDirty(resizePos, size);
    }

    public override void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
    {
        var dat = data;
        var w = width;
        var h = height;
        var pos = position;
        var fmt = format;
        var resizePos = new APoint((int)pos.X, (int)pos.Y);
        var size = new APoint(w, h);
        _drawActions.Add(new DrawAction(AutoResizeWidth || AutoResizeHeight, resizePos, size, _ =>
        {
            fmt.GetMasks(out uint rmask, out uint gmask, out uint bmask, out uint amask, out int bpp);
            var handle = GCHandle.Alloc(dat, GCHandleType.Pinned);
            nint surf = SDL.SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), w, h, bpp, w * (bpp / 8), rmask, gmask, bmask, amask);
            if (surf == nint.Zero) { handle.Free(); return; }
            nint tex = SDL.SDL_CreateTextureFromSurface(Renderer, surf);
            if (tex != nint.Zero)
            {
                SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)pos.X - OffsetX, y = (int)pos.Y - OffsetY, w = w, h = h };
                SDL.SDL_RenderCopy(Renderer, tex, nint.Zero, ref dst);
                SDL.SDL_DestroyTexture(tex);
            }
            SDL.SDL_FreeSurface(surf);
            handle.Free();
        }));
        MarkDirty(resizePos, size);
    }

    public override void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
    {
        var tex = texture;
        var w = width;
        var h = height;
        var pos = position;
        bool needResize = AutoResizeWidth || AutoResizeHeight;
        int calcW = w;
        int calcH = h;
        if (needResize)
        {
            switch (tex)
            {
                case SdlImageTexture surface when surface.SurfaceId != nint.Zero:
                    calcW = Math.Min(w, surface.Width);
                    calcH = Math.Min(h, surface.Height);
                    break;
                case SdlTexture2D img when img.Handle != nint.Zero:
                    calcW = Math.Min(w, img.Width);
                    calcH = Math.Min(h, img.Height);
                    break;
                default:
                    needResize = false;
                    break;
            }
        }
        var resizePos = new APoint((int)pos.X, (int)pos.Y);
        var size = new APoint(calcW, calcH);
        _drawActions.Add(new DrawAction(needResize, resizePos, size, _ =>
        {
            switch (tex)
            {
                case SdlImageTexture surface when surface.SurfaceId != nint.Zero:
                    {
                        nint sdlTex = SDL.SDL_CreateTextureFromSurface(Renderer, surface.SurfaceId);
                        if (sdlTex != nint.Zero)
                        {
                            SDL.SDL_Rect dst = new SDL.SDL_Rect
                            {
                                x = (int)pos.X - OffsetX,
                                y = (int)pos.Y - OffsetY,
                                w = w,
                                h = h
                            };
                            SDL.SDL_RenderCopy(Renderer, sdlTex, nint.Zero, ref dst);
                            SDL.SDL_DestroyTexture(sdlTex);
                        }
                        break;
                    }
                case SdlTexture2D img when img.Handle != nint.Zero:
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect
                        {
                            x = (int)pos.X - OffsetX,
                            y = (int)pos.Y - OffsetY,
                            w = w,
                            h = h
                        };
                        SDL.SDL_RenderCopy(Renderer, img.Handle, nint.Zero, ref dst);
                        break;
                    }
            }
        }));
        MarkDirty(resizePos, size);
    }

    public override IAbstTexture2D GetTexture(string? name = null)
    {
        if (UseTextureGrid)
            throw new NotSupportedException("UseTextureGrid enabled - retrieve tiles separately.");

        Render();
        var texture = new SdlTexture2D(_texture, Width, Height, name ?? $"Texture_{Width}x{Height}");
        if (_texture != nint.Zero)
        {
#if DEBUG
            texture.DebugWriteToDisk(Renderer);
#endif
        }
        else
        {
            // image too big, ignore
        }
        return texture;
    }
}
