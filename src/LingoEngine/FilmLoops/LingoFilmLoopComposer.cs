using System;
using System.Collections.Generic;
using LingoEngine.Bitmaps;
using LingoEngine.Core;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Shapes;
using LingoEngine.Sprites;
using LingoEngine.Texts;


namespace LingoEngine.FilmLoops;

/// <summary>
/// Provides helpers to convert a film loop and its sprite layers into a set of
/// prepared layer entries that backends can use to compose textures.
/// </summary>
public static class LingoFilmLoopComposer
{
    /// <summary>
    /// Describes a single layer prepared for film-loop composition. It captures
    /// source cropping and destination sizing information along with the
    /// transform and blending parameters required for rendering.
    /// </summary>
    public readonly record struct LayerInfo(
        //ILingoFrameworkMemberBitmap Bitmap,
        //ILingoImageTexture Texture,
        int SrcX,
        int SrcY,
        int SrcW,
        int SrcH,
        int DestW,
        int DestH,
        LingoTransform2D Transform,
        float Alpha,
        LingoInkType Ink,
        LingoColor BackColor, LingoSprite2DVirtual Sprite2D);

    /// <summary>
    /// Computes the overall bounds and per-layer data needed to render a film
    /// loop. The returned layers are ready for backend-specific blending using
    /// the supplied transforms and blend parameters.
    /// </summary>
    /// <param name="filmLoop">Film loop member being composed.</param>
    /// <param name="framing">How bitmap layers should be scaled or cropped.</param>
    /// <param name="layers">Virtual sprite layers that make up the film loop.</param>
    /// <returns>The offset to apply when rendering, the final width and height,
    /// and the prepared layer list.</returns>
    public static (LingoPoint Offset, int Width, int Height, List<LayerInfo> Layers) Prepare(LingoFilmLoopMember filmLoop, LingoFilmLoopFraming framing, IEnumerable<LingoSprite2DVirtual> layers)
    {
        var bounds = filmLoop.GetBoundingBox();
        var offset = new LingoPoint(-bounds.Left, -bounds.Top);
        int width = (int)MathF.Ceiling(bounds.Width);
        int height = (int)MathF.Ceiling(bounds.Height);
        var prepared = new List<LayerInfo>();

        //DebugInfo(layers, bounds, offset, width, height);

        foreach (var layer in layers)
        {
            int srcW = 0;
            int srcH = 0;
            if (layer.Member is ILingoMemberWithTexture)
            {
                srcW = (int)layer.Member.Width;
                srcH = (int)layer.Member.Height;
            }
            else
            {
                // filmloops
                if (layer.Texture == null) continue;
                srcW = (int)layer.Width;
                srcH = (int)layer.Height;
            }
            int destW = (int)layer.Width;
            int destH = (int)layer.Height;
            int srcX = 0;
            int srcY = 0;

            if (framing != LingoFilmLoopFraming.Scale)
            {
                int cropW = Math.Min(destW, srcW);
                int cropH = Math.Min(destH, srcH);
                srcX = (srcW - cropW) / 2;
                srcY = (srcH - cropH) / 2;
                srcW = cropW;
                srcH = cropH;
                destW = cropW;
                destH = cropH;
            }

            var reg = layer.RegPoint;
            if (layer.Member is { Width: > 0, Height: > 0 } m)
            {
                float scaleRegX = destW / (float)m.Width;
                float scaleRegY = destH / (float)m.Height;
                reg = new LingoPoint(reg.X * scaleRegX, reg.Y * scaleRegY);
            }
            else
            {
                reg = new LingoPoint(destW / 2f, destH / 2f);
            }

            var pos = new LingoPoint(layer.LocH + offset.X, layer.LocV + offset.Y);
            if (layer.Member is LingoFilmLoopMember nested)
            {
                var fl = nested.Framework<ILingoFrameworkMemberFilmLoop>();
                pos.X -= fl.Offset.X;
                pos.Y -= fl.Offset.Y;
            }
            var scale = new LingoPoint(layer.FlipH ? -1 : 1, layer.FlipV ? -1 : 1);
            var transform = LingoTransform2D.Identity
                .Translated(-reg.X, -reg.Y)
                .Scaled(scale.X, scale.Y)
                .Skewed(layer.Skew)
                .Rotated(layer.Rotation)
                .Translated(pos.X, pos.Y);

            prepared.Add(new LayerInfo(
                //bmp,
                srcX,
                srcY,
                srcW,
                srcH,
                destW,
                destH,
                transform,
#if NET48
                MathCompat.Clamp(layer.Blend / 100f, 0f, 1f),
#else
                Math.Clamp(layer.Blend / 100f, 0f, 1f),
#endif
                layer.InkType,
                layer.BackColor,
                layer));
        }

        return (offset, width, height, prepared);
    }
#if DEBUG
    private static void DebugInfo(IEnumerable<LingoSprite2DVirtual> layers, LingoRect bounds, LingoPoint offset, int width, int height)
    {
        Console.WriteLine("Running framework: " + LingoEngineGlobal.RunFramework);
        Console.WriteLine("bounds=\t" + bounds);
        Console.WriteLine("offset=\t" + offset);
        Console.WriteLine("size=\t" + width + "x" + height);
        foreach (var layer in layers)
        {
            Console.WriteLine("----- layer : " + layer.Member?.Name + "----");
            if (layer.Member != null)
            {
                Console.WriteLine($"Member.Size= {layer.Member.Width} x {layer.Member.Height}");
            }
            Console.WriteLine($"layer.Size=\t{layer.Width} x {layer.Height}");
            Console.WriteLine($"layer.RegPoint=\t{layer.RegPoint.X} x {layer.RegPoint.Y}");
            Console.WriteLine($"layer.Position=\t{layer.LocH} x {layer.LocV}");
            Console.WriteLine($"layer.Skew= \t{layer.Skew}");
            Console.WriteLine($"layer.Rotation=\t{layer.Rotation}");
        }
    }
#endif
}

