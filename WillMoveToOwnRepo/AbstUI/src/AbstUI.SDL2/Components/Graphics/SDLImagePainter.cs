using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.SDL2.Bitmaps;
using AbstUI.SDL2.SDLL;
using AbstUI.SDL2.Styles;
using AbstUI.Styles;
using AbstUI.Texts;
using System.Runtime.InteropServices;

namespace AbstUI.SDL2.Components.Graphics
{
    public class SDLImagePainter : IAbstImagePainter
    {
        private readonly SdlFontManager _fontManager;
        private nint _texture;
        private readonly List<(Func<APoint?> GetTotalSize, Action DrawAction)> _drawActions = new();
        private AColor? _clearColor;
        protected bool _dirty;
        private readonly int _maxWidth;
        private readonly int _maxHeight;

        public nint Renderer { get; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Pixilated { get; set; }
        public bool AutoResize { get; set; } = true;
        public nint Texture => _texture;

        public SDLImagePainter(IAbstFontManager fontManager, int width, int height, nint renderer)
        {
            _fontManager = (SdlFontManager)fontManager;
            (_maxWidth, _maxHeight) = GetMaxTexSize(renderer);
            Width = width > 0 ? Math.Min(width, _maxWidth) : 10;
            Height = height > 0 ? Math.Min(height, _maxHeight) : 10;

            _texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
            _dirty = true;
            Renderer = renderer;
        }

        /// <summary>
        /// Query once at startup
        /// </summary>
        public static (int W, int H) GetMaxTexSize(nint renderer)
        {
            SDL.SDL_GetRendererInfo(renderer, out var info);
            return ((int)info.max_texture_width, (int)info.max_texture_height);
        }
        public void Dispose()
        {
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
        }

        public void Render()
        {
            if (!_dirty) return;

            var newWidth = Width;
            var newHeight = Height;
            if (AutoResize)
            {
                foreach (var a in _drawActions)
                {
                    var newSize = a.GetTotalSize();
                    if (newSize != null && (newSize.Value.X > newWidth || newSize.Value.Y > newHeight))
                    {
                        newWidth = (int)MathF.Max(newWidth, newSize.Value.X);
                        newHeight = (int)MathF.Max(newHeight, newSize.Value.Y);
                    }
                }
            }
            newWidth = Math.Min(newWidth, _maxWidth);
            newHeight = Math.Min(newHeight, _maxHeight);
            if (newWidth > Width || newHeight > Height)
            {
                SDL.SDL_DestroyTexture(_texture);
                var nw = Math.Max(Width, newWidth);
                var nh = Math.Max(Height, newHeight);
                var newTex = SDL.SDL_CreateTexture(Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, nw, nh);
                _texture = newTex;
                Width = nw;
                Height = nh;
            }

            var prev = SDL.SDL_GetRenderTarget(Renderer);
            SDL.SDL_SetRenderTarget(Renderer, _texture);
            var clear = _clearColor ?? AColor.FromRGBA(0, 0, 0, 0);
            SDL.SDL_SetRenderDrawColor(Renderer, clear.R, clear.G, clear.B, clear.A);
            SDL.SDL_RenderClear(Renderer);
            foreach (var action in _drawActions)
                action.DrawAction();
            SDL.SDL_SetRenderTarget(Renderer, prev);
            _dirty = false;
        }


        protected void MarkDirty() => _dirty = true;

        public void Clear(AColor color)
        {
            _drawActions.Clear();
            _clearColor = color;
            MarkDirty();
        }

        public void SetPixel(APoint point, AColor color)
        {
            var p = point;
            var c = color;
            _drawActions.Add((
                () => AutoResize ? EnsureCapacity((int)p.X + 1, (int)p.Y + 1) : null,
                () =>
                {
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, 255);
                    SDL.SDL_RenderDrawPointF(Renderer, p.X, p.Y);
                }
            ));
            MarkDirty();
        }

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        {
            var s = start;
            var e = end;
            var c = color;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;
                    int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
                    int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
                    return EnsureCapacity(maxX, maxY);
                },
                () =>
                {
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, 255);
                    SDL.SDL_RenderDrawLineF(Renderer, s.X, s.Y, e.X, e.Y);
                }
            ));
            MarkDirty();
        }

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        {
            var r = rect;
            var c = color;
            var f = filled;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;
                    int x = (int)r.Left, y = (int)r.Top, w = (int)r.Width, h = (int)r.Height;
                    return EnsureCapacity(x + w, y + h);
                },
                () =>
                {
                    var rct = new SDL.SDL_Rect
                    {
                        x = (int)r.Left,
                        y = (int)r.Top,
                        w = (int)r.Width,
                        h = (int)r.Height
                    };
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
                    if (f)
                        SDL.SDL_RenderFillRect(Renderer, ref rct);
                    else
                        SDL.SDL_RenderDrawRect(Renderer, ref rct);
                }
            ));
            MarkDirty();
        }

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        {
            var ctr = center;
            var rad = radius;
            var c = color;
            var f = filled;
            _drawActions.Add((
                () => AutoResize ? EnsureCapacity((int)ctr.X + (int)MathF.Ceiling(rad) + 1, (int)ctr.Y + (int)MathF.Ceiling(rad) + 1) : null,
                () =>
                {
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
                    int segs = (int)(rad * 6);
                    double step = Math.PI * 2 / segs;
                    float prevX = ctr.X + rad;
                    float prevY = ctr.Y;
                    for (int i = 1; i <= segs; i++)
                    {
                        double angle = step * i;
                        float x = ctr.X + (float)(rad * Math.Cos(angle));
                        float y = ctr.Y + (float)(rad * Math.Sin(angle));
                        SDL.SDL_RenderDrawLineF(Renderer, prevX, prevY, x, y);
                        if (f)
                            SDL.SDL_RenderDrawLineF(Renderer, ctr.X, ctr.Y, x, y);
                        prevX = x;
                        prevY = y;
                    }
                }
            ));
            MarkDirty();
        }

        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        {
            var ctr = center;
            var rad = radius;
            var sd = startDeg;
            var ed = endDeg;
            var segs = segments;
            var c = color;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;
                    int cx = (int)ctr.X, cy = (int)ctr.Y, r = (int)MathF.Ceiling(rad);
                    return EnsureCapacity(cx + r + 1, cy + r + 1);
                },
                () =>
                {
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
                    double startRad = sd * Math.PI / 180.0;
                    double endRad = ed * Math.PI / 180.0;
                    double step = (endRad - startRad) / segs;
                    float prevX = ctr.X + (float)(rad * Math.Cos(startRad));
                    float prevY = ctr.Y + (float)(rad * Math.Sin(startRad));
                    for (int i = 1; i <= segs; i++)
                    {
                        double ang = startRad + i * step;
                        float x = ctr.X + (float)(rad * Math.Cos(ang));
                        float y = ctr.Y + (float)(rad * Math.Sin(ang));
                        SDL.SDL_RenderDrawLineF(Renderer, prevX, prevY, x, y);
                        prevX = x;
                        prevY = y;
                    }
                }
            ));
            MarkDirty();
        }

        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        {
            if (points.Count < 2) return;
            var pts = new APoint[points.Count];
            for (int i = 0; i < points.Count; i++)
                pts[i] = points[i];
            var c = color;
            var f = filled;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;
                    int maxX = 0, maxY = 0;
                    foreach (var p in pts)
                    {
                        if (p.X > maxX) maxX = (int)p.X;
                        if (p.Y > maxY) maxY = (int)p.Y;
                    }
                    return EnsureCapacity(maxX + 1, maxY + 1);
                },
                () =>
                {
                    SDL.SDL_SetRenderDrawColor(Renderer, c.R, c.G, c.B, c.A);
                    for (int i = 0; i < pts.Length - 1; i++)
                        SDL.SDL_RenderDrawLineF(Renderer, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
                    SDL.SDL_RenderDrawLineF(Renderer, pts[^1].X, pts[^1].Y, pts[0].X, pts[0].Y);
                    if (f)
                    {
                        var p0 = pts[0];
                        for (int i = 1; i < pts.Length - 1; i++)
                        {
                            SDL.SDL_RenderDrawLineF(Renderer, p0.X, p0.Y, pts[i].X, pts[i].Y);
                            SDL.SDL_RenderDrawLineF(Renderer, p0.X, p0.Y, pts[i + 1].X, pts[i + 1].Y);
                        }
                    }
                }
            ));
            MarkDirty();
        }

        public void DrawText(APoint position, string text, string? fontNamee = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default, AbstFontStyle style = AbstFontStyle.Regular)
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

            string[] lines = txt.Split('\n');
            int maxW = 0;
            int totalH = 0;
            foreach (var ln in lines)
            {
                var line = RenderLine(ln);
                if (SDL_ttf.TTF_SizeUTF8(fnt, line, out int tw, out int th) == 0)
                {
                    if (tw > maxW) maxW = tw;
                    totalH += th;
                }
            }

            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;

                    int needW = (w >= 0) ? Math.Max(maxW, w) : maxW;
                    return EnsureCapacity((int)pos.X + needW, (int)pos.Y + totalH);
                },
                () =>
                {
                    SDL.SDL_Color c = new SDL.SDL_Color { r = col?.R ?? 0, g = col?.G ?? 0, b = col?.B ?? 0, a = 255 };

                    List<(nint surf, int w, int h)> surfaces = new();
                    foreach (var ln in lines)
                    {
                        var line = RenderLine(ln);
                        nint s = SDL_ttf.TTF_RenderUTF8_Blended(fnt, line, c);
                        if (s == nint.Zero) continue;
                        var sur = SDL.PtrToStructure<SDL.SDL_Surface>(s);
                        surfaces.Add((s, sur.w, sur.h));
                    }

                    int y = (int)pos.Y;
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
                            SDL.SDL_Rect dst = new SDL.SDL_Rect { x = startX, y = y, w = tw, h = th };
                            SDL.SDL_RenderCopy(Renderer, tex, nint.Zero, ref dst);
                            SDL.SDL_DestroyTexture(tex);
                        }
                        SDL.SDL_FreeSurface(s);
                        y += th;
                    }
                }
            ));
            MarkDirty();
        }

        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        {
            var dat = data;
            var w = width;
            var h = height;
            var pos = position;
            var fmt = format;
            _drawActions.Add((
                () => AutoResize ? EnsureCapacity((int)pos.X + w, (int)pos.Y + h) : null,
                () =>
                {
                    fmt.GetMasks(out uint rmask, out uint gmask, out uint bmask, out uint amask, out int bpp);
                    var handle = GCHandle.Alloc(dat, GCHandleType.Pinned);
                    nint surf = SDL.SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), w, h, bpp, w * (bpp / 8), rmask, gmask, bmask, amask);
                    if (surf == nint.Zero) { handle.Free(); return; }
                    nint tex = SDL.SDL_CreateTextureFromSurface(Renderer, surf);
                    if (tex != nint.Zero)
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)pos.X, y = (int)pos.Y, w = w, h = h };
                        SDL.SDL_RenderCopy(Renderer, tex, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(tex);
                    }
                    SDL.SDL_FreeSurface(surf);
                    handle.Free();
                }
            ));
            MarkDirty();
        }
        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        {
            var tex = texture;
            var w = width;
            var h = height;
            var pos = position;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResize) return null;
                    switch (tex)
                    {
                        case SdlImageTexture surface when surface.SurfaceId != nint.Zero:
                            {
                                int copyW = Math.Min(w, surface.Width);
                                int copyH = Math.Min(h, surface.Height);
                                return EnsureCapacity((int)pos.X + copyW, (int)pos.Y + copyH);
                            }
                        case SdlTexture2D img when img.Handle != nint.Zero:
                            {
                                int copyW = Math.Min(w, img.Width);
                                int copyH = Math.Min(h, img.Height);
                                return EnsureCapacity((int)pos.X + copyW, (int)pos.Y + copyH);
                            }
                        default:
                            return null;
                    }
                },
                () =>
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
                                        x = (int)pos.X,
                                        y = (int)pos.Y,
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
                                    x = (int)pos.X,
                                    y = (int)pos.Y,
                                    w = w,
                                    h = h
                                };
                                SDL.SDL_RenderCopy(Renderer, img.Handle, nint.Zero, ref dst);
                                break;
                            }
                    }
                }
            ));
            MarkDirty();
        }

        public IAbstTexture2D GetTexture(string? name = null)
        {
            Render();
            var texture = new SdlTexture2D(_texture, Width, Height, name ?? $"Texture_{Width}x{Height}");
            if (_texture != nint.Zero)
            {
                //SdlTexture2D textureClone = (SdlTexture2D)texture.Clone(Renderer);
                //texture.Dispose();
                //texture.DebugWriteToDiskInc(Renderer);
            }
            else
            {
                // image to big, ignore
            }
            return texture;
        }

        private APoint? EnsureCapacity(int minW, int minH)
        {
            int newW = Math.Max(Width, minW);
            int newH = Math.Max(Height, minH);
            if (newW == Width && newH == Height) return null;
            return new APoint(newW, newH);
        }
    }
}
