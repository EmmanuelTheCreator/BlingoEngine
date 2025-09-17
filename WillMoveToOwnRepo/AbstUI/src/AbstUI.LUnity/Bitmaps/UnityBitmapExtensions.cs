using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Primitives;
using BlingoEngine.Tools;
using UnityEngine;
using AbstUI.Tools;
using Vec2 = System.Numerics.Vector2;

namespace AbstUI.LUnity.Bitmaps;

public static class UnityBitmapExtensions
{
    /// <summary>
    /// Fills the entire buffer with <paramref name="color"/>.
    /// </summary>
    public static void Fill(this Color32[] pix, int totalPixels, Color32 color)
    {
        ParallelHelper.For(0, totalPixels, totalPixels, i => pix[i] = color);
    }

    /// <summary>
    /// Draws a rectangle on the pixel buffer.
    /// </summary>
    public static void DrawRectangle(this Color32[] pix, int w, int h, Color32 fill,
        Color32 stroke, bool filled, int strokeWidth)
    {
        int totalPixels = w * h;
        if (filled)
            pix.Fill(totalPixels, fill);

        if (!filled || strokeWidth > 0)
        {
            for (int y = 0; y < strokeWidth && y < h; y++)
                for (int x = 0; x < w; x++)
                    pix[y * w + x] = stroke;
            for (int y = h - strokeWidth; y < h; y++)
                for (int x = 0; x < w; x++)
                    pix[y * w + x] = stroke;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < strokeWidth && x < w; x++)
                    pix[y * w + x] = stroke;
                for (int x = w - strokeWidth; x < w; x++)
                    if (x >= 0) pix[y * w + x] = stroke;
            }
        }
    }

    /// <summary>
    /// Draws an oval on the pixel buffer.
    /// </summary>
    public static void DrawOval(this Color32[] pix, int w, int h, Color32 fill,
        Color32 stroke, bool filled, int strokeWidth)
    {
        float cx = w / 2f;
        float cy = h / 2f;
        float rx = w / 2f;
        float ry = h / 2f;
        float maxR = MathF.Max(rx, ry);
        int totalPixels = w * h;

        ParallelHelper.For(0, h, totalPixels, y =>
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f - cx) / rx;
                float dy = (y + 0.5f - cy) / ry;
                float d = dx * dx + dy * dy;
                if (filled && d <= 1f)
                    pix[y * w + x] = fill;
                else if (!filled && Math.Abs(d - 1f) <= strokeWidth / maxR)
                    pix[y * w + x] = stroke;
            }
        });
    }

    /// <summary>
    /// Draws a single line using Bresenham's algorithm.
    /// </summary>
    public static void DrawLine(this Color32[] pix, int w, int h, int x0, int y0,
        int x1, int y1, Color32 color)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
                pix[y0 * w + x0] = color;
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>
    /// Draws a polyline defined by <paramref name="points"/>.
    /// </summary>
    public static void DrawPolyLine(this Color32[] pix, int w, int h,
        IReadOnlyList<APoint> points, bool closed, Color32 stroke)
    {
        if (points.Count < 2) return;
        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = points[i];
            var p1 = points[i + 1];
            pix.DrawLine(w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
        }
        if (closed)
        {
            var p0 = points[^1];
            var p1 = points[0];
            pix.DrawLine(w, h, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, stroke);
        }
    }

    /// <summary>
    /// Draws a rounded rectangle on the pixel buffer.
    /// </summary>
    public static void DrawRoundRect(this Color32[] pix, int w, int h, Color32 fill,
        Color32 stroke, bool filled, int strokeWidth)
    {
        int r = Math.Min(Math.Min(w, h) / 5, 20);
        int totalPixels = w * h;
        ParallelHelper.For(0, h, totalPixels, y =>
        {
            for (int x = 0; x < w; x++)
            {
                bool inside = true;
                if (x < r && y < r)
                    inside = (x - r) * (x - r) + (y - r) * (y - r) <= r * r;
                else if (x >= w - r && y < r)
                    inside = (x - (w - r - 1)) * (x - (w - r - 1)) + (y - r) * (y - r) <= r * r;
                else if (x < r && y >= h - r)
                    inside = (x - r) * (x - r) + (y - (h - r - 1)) * (y - (h - r - 1)) <= r * r;
                else if (x >= w - r && y >= h - r)
                    inside = (x - (w - r - 1)) * (x - (w - r - 1)) + (y - (h - r - 1)) * (y - (h - r - 1)) <= r * r;

                if (filled)
                {
                    if (inside)
                        pix[y * w + x] = fill;
                    else if (Math.Abs((x - r) * (x - r) + (y - r) * (y - r) - r * r) < r && x < r && y < r)
                        pix[y * w + x] = stroke;
                }
                else if (!inside && ((x >= r && x < w - r) || (y >= r && y < h - r)))
                {
                    pix[y * w + x] = stroke;
                }
            }
        });
    }

    /// <summary>
    /// Scales the source pixel buffer to the specified destination size using nearest-neighbor sampling.
    /// </summary>
    public static Color32[] ScalePixels(this Color32[] src, int srcW, int srcH, int destW, int destH)
    {
        var dst = new Color32[destW * destH];
        float ratioX = srcW / (float)destW;
        float ratioY = srcH / (float)destH;
        for (int y = 0; y < destH; y++)
        {
            int sy = (int)(y * ratioY);
            for (int x = 0; x < destW; x++)
            {
                int sx = (int)(x * ratioX);
                dst[y * destW + x] = src[sy * srcW + sx];
            }
        }
        return dst;
    }

    /// <summary>
    /// Draws <paramref name="src"/> onto the destination pixel buffer applying
    /// the provided transformation matrix and opacity.
    /// </summary>
    /// <param name="dest">Destination pixel buffer.</param>
    /// <param name="destWidth">Width of the destination buffer.</param>
    /// <param name="destHeight">Height of the destination buffer.</param>
    /// <param name="src">Source pixel buffer.</param>
    /// <param name="srcWidth">Width of the source buffer.</param>
    /// <param name="srcHeight">Height of the source buffer.</param>
    /// <param name="transform">Matrix that transforms points from source space into destination space.</param>
    /// <param name="alpha">Additional opacity applied to the source pixels.</param>
    public static void DrawWithMatrix(this Color32[] dest, int destWidth, int destHeight,
        Color32[] src, int srcWidth, int srcHeight, Matrix3x2 transform, float alpha)
    {
        Matrix3x2.Invert(transform, out var inv);

        Vec2[] pts =
        {
            Vec2.Transform(Vec2.Zero, transform),
            Vec2.Transform(new Vec2(srcWidth, 0), transform),
            Vec2.Transform(new Vec2(srcWidth, srcHeight), transform),
            Vec2.Transform(new Vec2(0, srcHeight), transform)
        };
        int minX = (int)MathF.Floor(pts[0].X);
        int maxX = (int)MathF.Ceiling(pts[0].X);
        int minY = (int)MathF.Floor(pts[0].Y);
        int maxY = (int)MathF.Ceiling(pts[0].Y);
        for (int i = 1; i < pts.Length; i++)
        {
            if (pts[i].X < minX) minX = (int)MathF.Floor(pts[i].X);
            if (pts[i].X > maxX) maxX = (int)MathF.Ceiling(pts[i].X);
            if (pts[i].Y < minY) minY = (int)MathF.Floor(pts[i].Y);
            if (pts[i].Y > maxY) maxY = (int)MathF.Ceiling(pts[i].Y);
        }

        int totalPixels = (maxX - minX) * (maxY - minY);
        ParallelHelper.For(minY, maxY, totalPixels, y =>
        {
            if ((uint)y >= (uint)destHeight) return;
            int row = y * destWidth;
            for (int x = minX; x < maxX; x++)
            {
                if ((uint)x >= (uint)destWidth) continue;
                var srcPos = Vec2.Transform(new Vec2(x + 0.5f, y + 0.5f), inv);
                int sx = (int)MathF.Floor(srcPos.X);
                int sy = (int)MathF.Floor(srcPos.Y);
                if ((uint)sx >= (uint)srcWidth || (uint)sy >= (uint)srcHeight)
                    continue;
                var c = src[sy * srcWidth + sx];
                float a = c.a / 255f * alpha;
                if (a <= 0f) continue;
                int idx = row + x;
                var dst = dest[idx];
                float invA = 1f - a;
                dest[idx] = new Color32(
                    (byte)(c.r * a + dst.r * invA),
                    (byte)(c.g * a + dst.g * invA),
                    (byte)(c.b * a + dst.b * invA),
                    (byte)(c.a * a + dst.a * invA));
            }
        });
    }
}



