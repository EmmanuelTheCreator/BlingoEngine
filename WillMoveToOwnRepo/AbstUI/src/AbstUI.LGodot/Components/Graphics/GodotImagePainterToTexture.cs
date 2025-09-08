using AbstUI.Bitmaps;
using AbstUI.Components.Graphics;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.LGodot.Styles;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using Godot;



namespace AbstUI.LGodot.Components.Graphics
{
    public class GodotImagePainterToTexture : IAbstImagePainter
    {
        private readonly List<(Func<APoint?> GetTotalSize, Action<Image> DrawAction)> _drawActions = new();
        private readonly AbstGodotFontManager _fontManager;
        private Image _img;
        private ImageTexture _tex;
        private AColor? _clearColor;
        private bool _dirty;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool Pixilated { get; set; }
        public Texture2D Texture => _tex;
        public bool AutoResizeWidth { get; set; } = false;
        public bool AutoResizeHeight { get; set; } = true;
        int IAbstImagePainter.Height { get => Height; set => Resize(Width, value); }
        int IAbstImagePainter.Width { get => Width; set => Resize(value, Height); }
        public string Name { get; set; } = "";

        public GodotImagePainterToTexture(AbstGodotFontManager fontManager, int width = 0, int height = 0)
        {
            _fontManager = fontManager;
            
            Width = width;
            Height = height;
            if (width == 0)
            {
                AutoResizeWidth = true;
                width = 10;
            }
            if (height == 0)
            {
                AutoResizeHeight = true;
                height = 10;
            }
            _img = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            _img.Fill(new Color(0, 0, 0, 0));
            _tex = ImageTexture.CreateFromImage(_img);
            _dirty = true;
        }

        public void Resize(int width, int height)
        {
            if (Width == width && Height == height)
                return;
            Width = width;
            Height = height;
            MarkDirty();
        }

        public void Dispose()
        {
            _tex?.Dispose();
            _img?.Dispose();
        }

        private void MarkDirty() => _dirty = true;
        public void Render()
        {
            if (!_dirty) return;
            if (_clearColor.HasValue)
            {
                var c = _clearColor.Value;
                _img.Fill(new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
            }
            var newWidth = Width>0? Width:10;
            var newHeight = Height>0?Height:10;
            if (AutoResizeWidth || AutoResizeHeight)
            {
                foreach (var a in _drawActions)
                {
                    var newSize = a.GetTotalSize();
                    if (newSize != null)
                    {
                        if (AutoResizeWidth && newSize.Value.X > newWidth)
                            newWidth = (int)MathF.Max(newWidth, newSize.Value.X);
                        if (AutoResizeHeight && newSize.Value.Y > newHeight)
                            newHeight = (int)MathF.Max(newHeight, newSize.Value.Y);
                    }
                }
            }
            if (newWidth == 0) newWidth = 10;
            if (newHeight == 0) newHeight = 10;
            if (newWidth > Width || newHeight > Height)
            {
                var nw = AutoResizeWidth ? Math.Max(Width, newWidth) : Width;
                var nh = AutoResizeHeight ? Math.Max(Height, newHeight) : Height;

                var newImg = Image.CreateEmpty(nw, nh, false, Image.Format.Rgba8);
                newImg.Fill(new Color(0, 0, 0, 0));
                newImg.BlitRect(_img, new Rect2I(0, 0, Width, Height), new Vector2I(0, 0));

                _img.Dispose();
                _img = newImg;

                _tex?.Dispose();
                _tex = ImageTexture.CreateFromImage(_img);

                Width = nw;
                Height = nh;
            }

            foreach (var a in _drawActions)
            {
                //Console.WriteLine(Width+"x"+Height+":"+a.DrawAction.Method.Name);
                a.DrawAction(_img);
            }

            _tex.Update(_img);
            _dirty = false;
        }
        public void Clear(AColor color)
        {
            _drawActions.Clear();
            _clearColor = color;
            MarkDirty();
        }

        public void SetPixel(APoint point, AColor color)
        {
            var p = point; var c = color;
            _drawActions.Add((
                () => (AutoResizeWidth || AutoResizeHeight) ? EnsureCapacity((int)p.X + 1, (int)p.Y + 1) : null,
                img =>
            {
                if ((uint)p.X < (uint)img.GetWidth() && (uint)p.Y < (uint)img.GetHeight())
                    img.SetPixel((int)p.X, (int)p.Y, new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
            }
            ));
            MarkDirty();
        }

        public void DrawLine(APoint start, APoint end, AColor color, float width = 1)
        {
            var s = start; var e = end; var col = color;
            _drawActions.Add((() =>
            {
                if (!AutoResizeWidth && !AutoResizeHeight) return null;
                int maxX = (int)MathF.Ceiling(MathF.Max(s.X, e.X)) + 1;
                int maxY = (int)MathF.Ceiling(MathF.Max(s.Y, e.Y)) + 1;
                return EnsureCapacity(maxX, maxY);
            }
            ,
            img =>
            {
                var c = new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
                int x0 = (int)s.X, y0 = (int)s.Y, x1 = (int)e.X, y1 = (int)e.Y;
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = dx + dy, e2;
                while (true)
                {
                    if ((uint)x0 < (uint)img.GetWidth() && (uint)y0 < (uint)img.GetHeight())
                        img.SetPixel(x0, y0, c);
                    if (x0 == x1 && y0 == y1) break;
                    e2 = 2 * err;
                    if (e2 >= dy) { err += dy; x0 += sx; }
                    if (e2 <= dx) { err += dx; y0 += sy; }
                }
            }
            ));
            MarkDirty();
        }

        public void DrawRect(ARect rect, AColor color, bool filled = true, float width = 1)
        {
            var r = rect; var col = color; var f = filled;
            _drawActions.Add((() =>
            {
                if (!AutoResizeWidth && !AutoResizeHeight) return null;
                int x = (int)r.Left, y = (int)r.Top, w = (int)r.Width, h = (int)r.Height;
                return EnsureCapacity(x + w, y + h);
            }, img =>
            {
                var c = new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
                int x = (int)r.Left, y = (int)r.Top, w = (int)r.Width, h = (int)r.Height;

                if (w <= 0 || h <= 0) return;
                if (f)
                {
                    for (int j = 0; j < h; j++)
                        for (int i = 0; i < w; i++)
                        {
                            int px = x + i, py = y + j;
                            if ((uint)px < (uint)img.GetWidth() && (uint)py < (uint)img.GetHeight())
                                img.SetPixel(px, py, c);
                        }
                }
                else
                {
                    for (int i = 0; i < w; i++)
                    {
                        int px = x + i;
                        if ((uint)px < (uint)img.GetWidth())
                        {
                            if ((uint)y < (uint)img.GetHeight()) img.SetPixel(px, y, c);
                            int by = y + h - 1; if ((uint)by < (uint)img.GetHeight()) img.SetPixel(px, by, c);
                        }
                    }
                    for (int j = 0; j < h; j++)
                    {
                        int py = y + j;
                        if ((uint)py < (uint)img.GetHeight())
                        {
                            if ((uint)x < (uint)img.GetWidth()) img.SetPixel(x, py, c);
                            int rx = x + w - 1; if ((uint)rx < (uint)img.GetWidth()) img.SetPixel(rx, py, c);
                        }
                    }
                }
            }
            ));
            MarkDirty();
        }

        public void DrawCircle(APoint center, float radius, AColor color, bool filled = true, float width = 1)
        {
            var ctr = center; var rad = Math.Max(0, radius); var col = color; var f = filled;
            int cx = (int)ctr.X, cy = (int)ctr.Y, r = (int)Math.Round(rad);
            _drawActions.Add((
                () => (AutoResizeWidth || AutoResizeHeight) ? EnsureCapacity(cx + r + 1, cy + r + 1) : null,
                img =>
            {
                var c = new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);

                int x = r, y = 0, err = 0;
                void Plot(int px, int py) { if ((uint)px < (uint)img.GetWidth() && (uint)py < (uint)img.GetHeight()) img.SetPixel(px, py, c); }
                while (x >= y)
                {
                    if (f)
                    {
                        for (int xi = cx - x; xi <= cx + x; xi++) { Plot(xi, cy + y); Plot(xi, cy - y); }
                        for (int xi = cx - y; xi <= cx + y; xi++) { Plot(xi, cy + x); Plot(xi, cy - x); }
                    }
                    else
                    {
                        Plot(cx + x, cy + y); Plot(cx + y, cy + x); Plot(cx - x, cy + y); Plot(cx - y, cy + x);
                        Plot(cx - x, cy - y); Plot(cx - y, cy - x); Plot(cx + x, cy - y); Plot(cx + y, cy - x);
                    }
                    y++; err += 1 + 2 * y;
                    if (2 * (err - x) + 1 > 0) { x--; err += 1 - 2 * x; }
                }
            }
            ));
            MarkDirty();
        }

        public void DrawArc(APoint center, float radius, float startDeg, float endDeg, int segments, AColor color, float width = 1)
        {
            var ctr = center; var rad = radius; var sd = startDeg; var ed = endDeg; var segs = Math.Max(1, segments); var col = color;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResizeWidth && !AutoResizeHeight) return null;
                    int cx = (int)ctr.X, cy = (int)ctr.Y, r = (int)MathF.Ceiling(rad);
                    return EnsureCapacity(cx + r + 1, cy + r + 1);
                },
                img =>
            {
                var c = new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
                double a0 = sd * Math.PI / 180.0;
                double a1 = ed * Math.PI / 180.0;
                double step = (a1 - a0) / segs;
                int px = (int)(ctr.X + rad * Math.Cos(a0));
                int py = (int)(ctr.Y + rad * Math.Sin(a0));
                for (int i = 1; i <= segs; i++)
                {
                    double a = a0 + i * step;
                    int x = (int)(ctr.X + rad * Math.Cos(a));
                    int y = (int)(ctr.Y + rad * Math.Sin(a));
                    // reuse line drawer inline
                    int x0 = px, y0 = py, x1 = x, y1 = y;
                    int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                    int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                    int err = dx + dy, e2;
                    while (true)
                    {
                        if ((uint)x0 < (uint)img.GetWidth() && (uint)y0 < (uint)img.GetHeight())
                            img.SetPixel(x0, y0, c);
                        if (x0 == x1 && y0 == y1) break;
                        e2 = 2 * err;
                        if (e2 >= dy) { err += dy; x0 += sx; }
                        if (e2 <= dx) { err += dx; y0 += sy; }
                    }
                    px = x; py = y;
                }
            }
            ));
            MarkDirty();
        }

        public void DrawPolygon(IReadOnlyList<APoint> points, AColor color, bool filled = true, float width = 1)
        {
            if (points == null || points.Count < 2) return;
            var pts = new APoint[points.Count]; for (int i = 0; i < points.Count; i++) pts[i] = points[i];
            var col = color; var f = filled;
            _drawActions.Add((
                () =>
                {
                    if (!AutoResizeWidth && !AutoResizeHeight) return null;
                    int maxX = 0, maxY = 0;
                    foreach (var p in pts) { if (p.X > maxX) maxX = (int)p.X; if (p.Y > maxY) maxY = (int)p.Y; }
                    return EnsureCapacity(maxX + 1, maxY + 1);
                },
                img =>
            {
                var c = new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
                // outline
                for (int i = 0; i < pts.Length; i++)
                {
                    var a = pts[i]; var b = pts[(i + 1) % pts.Length];
                    // draw line a->b
                    int x0 = (int)a.X, y0 = (int)a.Y, x1 = (int)b.X, y1 = (int)b.Y;
                    int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                    int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                    int err = dx + dy, e2;
                    while (true)
                    {
                        if ((uint)x0 < (uint)img.GetWidth() && (uint)y0 < (uint)img.GetHeight())
                            img.SetPixel(x0, y0, c);
                        if (x0 == x1 && y0 == y1) break;
                        e2 = 2 * err;
                        if (e2 >= dy) { err += dy; x0 += sx; }
                        if (e2 <= dx) { err += dx; y0 += sy; }
                    }
                }
                if (!f) return;

                // simple scanline fill
                int minY = int.MaxValue, maxY = int.MinValue;
                foreach (var p in pts) { minY = Math.Min(minY, (int)p.Y); maxY = Math.Max(maxY, (int)p.Y); }
                minY = Math.Max(minY, 0); maxY = Math.Min(maxY, img.GetHeight() - 1);
                for (int y = minY; y <= maxY; y++)
                {
                    List<int> nodes = new();
                    int j = pts.Length - 1;
                    for (int i = 0; i < pts.Length; i++)
                    {
                        float yi = pts[i].Y, yj = pts[j].Y;
                        float xi = pts[i].X, xj = pts[j].X;
                        if ((yi < y && yj >= y) || (yj < y && yi >= y))
                        {
                            int x = (int)(xi + (y - yi) / (yj - yi) * (xj - xi));
                            nodes.Add(x);
                        }
                        j = i;
                    }
                    nodes.Sort();
                    for (int k = 0; k + 1 < nodes.Count; k += 2)
                    {
                        int xStart = Math.Max(nodes[k], 0);
                        int xEnd = Math.Min(nodes[k + 1], img.GetWidth() - 1);
                        for (int x = xStart; x <= xEnd; x++)
                            img.SetPixel(x, y, c);
                    }
                }
            }
            ));
            MarkDirty();
        }


        #region Draw Text

        public void DrawText(APoint position, string text, string? fontName = null, AColor? color = null,
                      int fontSize = 12, int width = -1, AbstTextAlignment alignment = default,
                      AbstFontStyle style = AbstFontStyle.Regular)
        {
            var pos = position;
            var txt = text ?? string.Empty;
            var fntName = fontName;
            var col = color ?? new AColor(0, 0, 0, 255);
            var fs = Math.Max(1, fontSize);
            int boxWCache = -1; // computed width for alignment when AutoResizeWidth
            var font = _fontManager.GetTypedOrDefault(fntName ?? string.Empty, style);
            if (font == null || string.IsNullOrEmpty(txt)) return;
            fntName = font.FontName;

            _drawActions.Add((
                // measure pass
                () =>
                {
                    return DrawTextSize(width, style, pos, txt, font, fntName, fs, ref boxWCache);
                },
                // draw pass
                img =>
                {
                    bool flowControl = DrawTextInner(alignment, style, img, pos, txt, font, fntName, col, fs, boxWCache);
                    if (!flowControl)
                    {
                        return;
                    }
                }
            ));

            MarkDirty();
        }

        private bool DrawTextInner(AbstTextAlignment alignment, AbstFontStyle style, Image img, APoint pos, string txt, FontFile font, string? fntName, AColor col, int fs, int boxWCache)
        {

            var ts = TextServerManager.GetPrimaryInterface();
            var rids = font.GetRids();

            int y = (int)pos.Y;
            foreach (var line in txt.Split('\n'))
            {
                // measure vertical advance
                var shaped = ts.CreateShapedText();
                ts.ShapedTextAddString(shaped, line, rids, fs);
                ts.ShapedTextShape(shaped);
                int advH = (int)MathF.Ceiling((float)(ts.ShapedTextGetAscent(shaped) + ts.ShapedTextGetDescent(shaped)));
                ts.FreeRid(shaped);

                // reuse single-line renderer with computed box width
                DrawSingleLineInner(alignment, style, img, new APoint(pos.X, y), line, font, fntName!, col, fs, boxWCache);

                y += advH;
            }

            return true;
        }

        private APoint? DrawTextSize(int width, AbstFontStyle style, APoint pos, string txt,
                             FontFile font, string fntName, int fs, ref int boxWCache)
        {
            if ((!AutoResizeWidth && !AutoResizeHeight) || string.IsNullOrEmpty(txt)) return null;

            var ts = TextServerManager.GetPrimaryInterface();
            var rids = font.GetRids();

            int maxLineW = 0, totalH = 0;
            foreach (var line in txt.Split('\n'))
            {
                var shaped = ts.CreateShapedText();
                ts.ShapedTextAddString(shaped, line, rids, fs);
                ts.ShapedTextShape(shaped);

                maxLineW = Math.Max(maxLineW, (int)MathF.Ceiling(ts.ShapedTextGetSize(shaped).X));
                totalH += (int)MathF.Ceiling((float)(ts.ShapedTextGetAscent(shaped) + ts.ShapedTextGetDescent(shaped)));

                ts.FreeRid(shaped);
            }

            // when width < 0: align against current canvas width (remaining space), not just maxLineW
            int canvasW = Math.Max(0, Width - (int)pos.X);
            boxWCache = (width >= 0) ? Math.Max(width, maxLineW) : Math.Max(canvasW, maxLineW);

            return EnsureCapacity((int)pos.X + boxWCache, (int)pos.Y + totalH);
        }


        #endregion

        #region Single line

        public void DrawSingleLine(APoint position, string text, string? fontName = null, AColor? color = null, int fontSize = 12, int width = -1, int height = -1, AbstTextAlignment alignment = default, AbstFontStyle style = AbstFontStyle.Regular)
        {
            var pos = position;
            var txt = text ?? string.Empty;
            var fntName = fontName;
            var col = color ?? new AColor(0, 0, 0, 255);
            var fs = Math.Max(1, fontSize);
            var w = width;
            var h = height;
            var font = _fontManager.GetTypedOrDefault(fntName ?? string.Empty, style);
            if (font == null || string.IsNullOrEmpty(txt)) return;
            fntName = font.FontName;

            _drawActions.Add((
                () =>
                {
                    return DrawSingleLineCalculateSize(style, pos, txt, font, fntName, fs, w, h);
                },
                img =>
                {
                    bool flowControl = DrawSingleLineInner(alignment, style, img, pos, txt, font, fntName, col, fs, w);
                    if (!flowControl)
                    {
                        return;
                    }
                }
            ));
            MarkDirty();
        }

        private APoint? DrawSingleLineCalculateSize(AbstFontStyle style, APoint pos, string txt, FontFile font, string? fntName, int fs, int w, int h)
        {
            // Auto-resize needs baseline offset too
            if (!AutoResizeWidth && !AutoResizeHeight) return null;
            if (w >= 0 && h >= 0)
                return EnsureCapacity((int)pos.X + w, (int)pos.Y + h);

            int needW = w;
            int needH = h >= 0 ? h : fs;

            if (font != null && !string.IsNullOrEmpty(txt) && (w < 0 || h < 0))
            {
                var rids = font.GetRids();
                var fr = (Rid)rids[0];
                var ts = TextServerManager.GetPrimaryInterface();

                var shaped = ts.CreateShapedText();
                ts.ShapedTextAddString(shaped, txt, rids, fs);
                ts.ShapedTextShape(shaped);

                var lineSize = ts.ShapedTextGetSize(shaped);
                if (w < 0) needW = (int)MathF.Ceiling(lineSize.X);

                if (h < 0)
                {
                    var ascent = ts.FontGetAscent(fr, fs);
                    var descent = ts.FontGetDescent(fr, fs);
                    var baseFrac = ts.FontGetBaselineOffset(fr); // 0..1 of font height
                    var fontH = ascent + descent;
                    var totalH = fontH + MathF.Abs((float)(baseFrac * fontH));
                    needH = (int)MathF.Ceiling((float)totalH);
                }

                ts.FreeRid(shaped);
            }
            else
            {
                if (w < 0) needW = 0;
            }

            return EnsureCapacity(
                (int)pos.X + Math.Max(0, needW),
                (int)pos.Y + Math.Max(0, needH)
            );
        }

        private bool DrawSingleLineInner(AbstTextAlignment alignment, AbstFontStyle style, Image img, APoint pos, string txt, FontFile font, string fntName, AColor col, int fs, int w)
        {

            var sizeKey = new Vector2I(fs, 0);
            var rids = font.GetRids();
            var fr = (Rid)rids[0];
            var atlasCache = _fontManager.GetAtlasCache(fntName!, fs);
            var tint = col.ToGodotColor();
            var imgW = (uint)img.GetWidth();
            var imgH = (uint)img.GetHeight();
            var ts = TextServerManager.GetPrimaryInterface();

            var shaped = ts.CreateShapedText();
            ts.ShapedTextAddString(shaped, txt, rids, fs);
            ts.ShapedTextShape(shaped);

            var lineSize = ts.ShapedTextGetSize(shaped);
            float lineW = lineSize.X;
            // 1) Compute baseline correctly (FontGetBaselineOffset is a FRACTION of font height)
            var ascent = ts.FontGetAscent(fr, fs);
            var descent = ts.FontGetDescent(fr, fs);
            var baseFrac = ts.FontGetBaselineOffset(fr); // 0..1 of font height
            var baseline = (int)MathF.Floor((float)(pos.Y + ascent + baseFrac * (ascent + descent)));

            int boxW = (w >= 0) ? w : Math.Max(0, (int)imgW - (int)pos.X); // auto width = remaining canvas
            float xOff = 0f;
            if (alignment == AbstTextAlignment.Center) xOff = MathF.Max(0, (boxW - lineW) * 0.5f);
            else if (alignment == AbstTextAlignment.Right) xOff = MathF.Max(0, boxW - lineW);

            double penX = 0.0;
            foreach (Godot.Collections.Dictionary g in ts.ShapedTextGetGlyphs(shaped))
            {
                int glyphIndex = (int)g["index"];
                float advance = (float)g["advance"];

                // optional sub-pixel shaping offset from the shaper:
                var shapeOff = (Vector2)g["offset"];

                // static glyph offset/bearing from the font:
                var glyphOff = ts.FontGetGlyphOffset(fr, sizeKey, glyphIndex);

                var texIdx = ts.FontGetGlyphTextureIdx(fr, sizeKey, glyphIndex);
                if (texIdx < 0) { penX += advance; continue; }

                if (!atlasCache.TryGetValue(texIdx, out var atlas))
                {
                    var aimg = ts.FontGetTextureImage(fr, sizeKey, texIdx);
                    if (aimg == null || aimg.IsEmpty()) { penX += advance; continue; }
                    aimg.Convert(Image.Format.Rgba8);
                    atlasCache[texIdx] = aimg;
                }
                var srcImg = atlasCache[texIdx];

                var uv = ts.FontGetGlyphUVRect(fr, sizeKey, glyphIndex);
                int sx = (int)uv.Position.X, sy = (int)uv.Position.Y;
                int sw = (int)uv.Size.X, sh = (int)uv.Size.Y;
                if (sw <= 0 || sh <= 0) { penX += advance; continue; }

                // 3) Place bitmap at pen + offsets (no “- advance”, we advance AFTER drawing)
                int dx = (int)MathF.Floor((float)(pos.X + xOff + penX + shapeOff.X + glyphOff.X));
                int dy = (int)MathF.Floor((float)(baseline + shapeOff.Y + glyphOff.Y));

                // blit...
                for (int yy = 0; yy < sh; yy++)
                {
                    int ty = dy + yy; if ((uint)ty >= imgH) continue;
                    for (int xx = 0; xx < sw; xx++)
                    {
                        int tx = dx + xx; if ((uint)tx >= imgW) continue;
                        var sp = srcImg.GetPixel(sx + xx, sy + yy);
                        float a = sp.A * tint.A; if (a <= 0f) continue;
                        img.SetPixel(tx, ty, new Color(tint.R, tint.G, tint.B, a));
                    }
                }

                penX += advance; // 4) advance AFTER drawing
            }

            ts.FreeRid(shaped);
            return true;
        }

        #endregion


        private static string TrimToWidth(string line, int maxW, TextServer ts, Godot.Collections.Array<Rid> rids, long sizeKey)
        {
            if (maxW < 0 || string.IsNullOrEmpty(line)) return line;
            // naive right-trim until shaped width fits
            // (fast enough for HUD text; optimize if needed)
            while (line.Length > 0)
            {
                var shaped = ts.CreateShapedText();
                ts.ShapedTextAddString(shaped, line, rids, sizeKey);
                ts.ShapedTextShape(shaped);
                var sz = ts.ShapedTextGetSize(shaped).X;
                ts.FreeRid(shaped);
                if (sz <= maxW) break;
                line = line[..^1];
            }
            return line;
        }

        public void DrawPicture(byte[] data, int width, int height, APoint position, APixelFormat format)
        {
            var dat = data; var w = width; var h = height; var pos = position; var fmt = format;
            _drawActions.Add((
                () => (AutoResizeWidth || AutoResizeHeight) ? EnsureCapacity((int)pos.X + w, (int)pos.Y + h) : null,
                img =>
            {
                var src = Image.CreateFromData(w, h, false, fmt.ToGodotFormat(), dat);
                if (src.GetFormat() != img.GetFormat())
                    src.Convert(img.GetFormat());
                img.BlitRect(src, new Rect2I(0, 0, w, h), new Vector2I((int)pos.X, (int)pos.Y));
                src.Dispose();
            }
            ));
            MarkDirty();
        }

        public void DrawPicture(IAbstTexture2D texture, int width, int height, APoint position)
        {
            var pos = position; var w = width; var h = height;
            switch (texture)
            {
                case AbstGodotTexture2D gtex:
                    _drawActions.Add((
                         () =>
                         {
                             if (!AutoResizeWidth && !AutoResizeHeight) return null;
                             int gw = gtex.Texture.GetWidth();
                             int gh = gtex.Texture.GetHeight();
                             int copyW = Math.Min(w, gw);
                             int copyH = Math.Min(h, gh);
                             return EnsureCapacity((int)pos.X + copyW, (int)pos.Y + copyH);
                         },
                         img =>
                    {
                        using var srcImg = gtex.Texture.GetImage();
                        if (srcImg.GetFormat() != img.GetFormat())
                            srcImg.Convert(img.GetFormat());
                        img.BlitRect(srcImg, new Rect2I(0, 0, Math.Min(w, srcImg.GetWidth()), Math.Min(h, srcImg.GetHeight())),
                                     new Vector2I((int)pos.X, (int)pos.Y));
                    }
                    ));
                    break;
                default:
                    throw new NotSupportedException("Unsupported texture type for GodotImagePainter.");
            }
            MarkDirty();
        }

        public IAbstTexture2D GetTexture(string? name = null)
        {
            Render();
            // ImageTexture already updated
            var texture = new AbstGodotTexture2D(_tex, name ?? $"GodotImage_{Width}x{Height}");
#if DEBUG
            texture.DebugWriteToDisk();
#endif
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



