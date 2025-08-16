using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using AbstUI.Styles;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Texts;
using AbstUI.ImGui.Bitmaps;
using AbstUI.ImGui.ImGuiLL;

namespace AbstUI.ImGui.Components
{
    internal class AbstImGuiGfxCanvas : AbstImGuiComponent, IAbstFrameworkGfxCanvas, IDisposable
    {
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly AbstImGuiComponentFactory _factory;
        private readonly IAbstFontManager _fontManager;
        private readonly int _width;
        private readonly int _height;
        private nint _texture;
        private readonly nint _imguiTexture;
        private readonly List<Action> _drawActions = new();
        private AColor? _clearColor;
        private bool _dirty;
        public object FrameworkNode => this;
        public nint Texture => _texture;

        public bool Pixilated { get; set; }

        public AbstImGuiGfxCanvas(AbstImGuiComponentFactory factory, IAbstFontManager fontManager, int width, int height) : base(factory)
        {
            _factory = factory;
            _fontManager = fontManager;
            _width = width;
            _height = height;
            _texture = SDL.SDL_CreateTexture(ComponentContext.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
            _imguiTexture = factory.RootContext.RegisterTexture(_texture);
            Width = width;
            Height = height;
            _dirty = true;
        }
        public override void Dispose()
        {
            base.Dispose();
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
        }

        public override AbstImGuiRenderResult Render(AbstImGuiRenderContext context)
        {
            if (!Visibility)
                return nint.Zero;

            ComponentContext.Renderer = context.Renderer;

            if (_dirty)
            {
                var prev = SDL.SDL_GetRenderTarget(ComponentContext.Renderer);
                SDL.SDL_SetRenderTarget(ComponentContext.Renderer, _texture);
                var clear = _clearColor ?? new AColor(0, 0, 0);
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, clear.R, clear.G, clear.B, 255);
                SDL.SDL_RenderClear(ComponentContext.Renderer);
                foreach (var action in _drawActions)
                    action();
                SDL.SDL_SetRenderTarget(ComponentContext.Renderer, prev);
                //SDL.BlendIm
                _dirty = false;
            }

            var screenPos = context.Origin + new Vector2(X, Y);
            global::ImGuiNET.ImGui.SetCursorScreenPos(screenPos);
            global::ImGuiNET.ImGui.PushID(Name);
            var imguiTexture = _factory.RootContext.GetTexture(_imguiTexture);
            global::ImGuiNET.ImGui.Image(imguiTexture, new Vector2(Width, Height));
            global::ImGuiNET.ImGui.PopID();

            return AbstImGuiRenderResult.RequireRender();
        }


        private void MarkDirty() => _dirty = true;

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
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
                SDL.SDL_RenderDrawPointF(ComponentContext.Renderer, p.X, p.Y);
            });
            MarkDirty();
        }

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        {
            var s = start;
            var e = end;
            var c = color;
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
                SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, s.X, s.Y, e.X, e.Y);
            });
            MarkDirty();
        }

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        {
            var rct = new SDL.SDL_Rect
            {
                x = (int)rect.Left,
                y = (int)rect.Top,
                w = (int)rect.Width,
                h = (int)rect.Height
            };
            var c = color;
            var f = filled;
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
                if (f)
                    SDL.SDL_RenderFillRect(ComponentContext.Renderer, ref rct);
                else
                    SDL.SDL_RenderDrawRect(ComponentContext.Renderer, ref rct);
            });
            MarkDirty();
        }

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        {
            var ctr = center;
            var rad = radius;
            var c = color;
            var f = filled;
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
                int segs = (int)(rad * 6);
                double step = Math.PI * 2 / segs;
                float prevX = ctr.X + rad;
                float prevY = ctr.Y;
                for (int i = 1; i <= segs; i++)
                {
                    double angle = step * i;
                    float x = ctr.X + (float)(rad * Math.Cos(angle));
                    float y = ctr.Y + (float)(rad * Math.Sin(angle));
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, prevX, prevY, x, y);
                    if (f)
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, ctr.X, ctr.Y, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
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
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
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
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, prevX, prevY, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
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
            _drawActions.Add(() =>
            {
                SDL.SDL_SetRenderDrawColor(ComponentContext.Renderer, c.R, c.G, c.B, 255);
                for (int i = 0; i < pts.Length - 1; i++)
                    SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
                SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, pts[^1].X, pts[^1].Y, pts[0].X, pts[0].Y);
                if (f)
                {
                    var p0 = pts[0];
                    for (int i = 1; i < pts.Length - 1; i++)
                    {
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, p0.X, p0.Y, pts[i].X, pts[i].Y);
                        SDL.SDL_RenderDrawLineF(ComponentContext.Renderer, p0.X, p0.Y, pts[i + 1].X, pts[i + 1].Y);
                    }
                }
            });
            MarkDirty();
        }

        public void DrawText(APoint position, string text, string? font = null, AColor? color = null, int fontSize = 12, int width = -1, AbstTextAlignment alignment = default)
        {
            var pos = position;
            var txt = text;
            var fntName = font;
            var col = color;
            var fs = fontSize;
            var w = width;
            _drawActions.Add(() =>
            {
                var path = _fontManager.Get<string>(fntName ?? string.Empty);
                if (string.IsNullOrEmpty(path)) return;
                nint fnt = SDL_ttf.TTF_OpenFont(path, fs);
                if (fnt == nint.Zero) return;
                SDL.SDL_Color c = new SDL.SDL_Color { r = col?.R ?? 0, g = col?.G ?? 0, b = col?.B ?? 0, a = 255 };

                string RenderLine(string line)
                {
                    if (w >= 0)
                    {
                        while (line.Length > 0 && SDL_ttf.TTF_SizeUTF8(fnt, line, out int tw, out _) == 0 && tw > w)
                        {
                            line = line.Substring(0, line.Length - 1);
                        }
                    }
                    return line;
                }

                string[] lines = txt.Split('\n');
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
                foreach (var (s, tw, th) in surfaces)
                {
                    nint tex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, s);
                    if (tex != nint.Zero)
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)pos.X, y = y, w = tw, h = th };
                        SDL.SDL_RenderCopy(ComponentContext.Renderer, tex, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(tex);
                    }
                    SDL.SDL_FreeSurface(s);
                    y += th;
                }

                SDL_ttf.TTF_CloseFont(fnt);
            });
            MarkDirty();
        }

        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        {
            var dat = data;
            var w = width;
            var h = height;
            var pos = position;
            var fmt = format;
            _drawActions.Add(() =>
            {
                fmt.GetMasks(out uint rmask, out uint gmask, out uint bmask, out uint amask, out int bpp);
                var handle = GCHandle.Alloc(dat, GCHandleType.Pinned);
                nint surf = SDL.SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), w, h, bpp, w * (bpp / 8), rmask, gmask, bmask, amask);
                if (surf == nint.Zero) { handle.Free(); return; }
                nint tex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, surf);
                if (tex != nint.Zero)
                {
                    SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)pos.X, y = (int)pos.Y, w = w, h = h };
                    SDL.SDL_RenderCopy(ComponentContext.Renderer, tex, nint.Zero, ref dst);
                    SDL.SDL_DestroyTexture(tex);
                }
                SDL.SDL_FreeSurface(surf);
                handle.Free();
            });
            MarkDirty();
        }
        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        {
            var tex = texture;
            var w = width;
            var h = height;
            var pos = position;
            _drawActions.Add(() =>
            {
                if (tex is ImGuiTexture2D img)
                {

                    nint sdlTex = SDL.SDL_CreateTextureFromSurface(ComponentContext.Renderer, img.Handle);
                    if (sdlTex != nint.Zero)
                    {
                        SDL.SDL_Rect dst = new SDL.SDL_Rect
                        {
                            x = (int)pos.X,
                            y = (int)pos.Y,
                            w = w,
                            h = h
                        };
                        SDL.SDL_RenderCopy(ComponentContext.Renderer, sdlTex, nint.Zero, ref dst);
                        SDL.SDL_DestroyTexture(sdlTex);
                    }
                }
            });
            MarkDirty();
        }



    }
}
