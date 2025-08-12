using System;
using System.Collections.Generic;
using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

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
        ILingoFrameworkMemberBitmap Bitmap,
        int SrcX,
        int SrcY,
        int SrcW,
        int SrcH,
        int DestW,
        int DestH,
        LingoTransform2D Transform,
        float Alpha,
        LingoInkType Ink,
        LingoColor BackColor);

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
    public static (LingoPoint Offset, int Width, int Height, List<LayerInfo> Layers)
        Prepare(LingoFilmLoopMember filmLoop, LingoFilmLoopFraming framing, IEnumerable<LingoSprite2DVirtual> layers)
    {
        var bounds = filmLoop.GetBoundingBox();
        var offset = new LingoPoint(-bounds.Left, -bounds.Top);
        int width = (int)MathF.Ceiling(bounds.Width);
        int height = (int)MathF.Ceiling(bounds.Height);
        var prepared = new List<LayerInfo>();

        foreach (var layer in layers)
        {
            if (layer.Member is not LingoMemberBitmap pic)
                continue;
            var bmp = pic.Framework<ILingoFrameworkMemberBitmap>();
            int srcW = bmp.Width;
            int srcH = bmp.Height;
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

            var srcCenter = new LingoPoint(destW / 2f, destH / 2f);
            var pos = new LingoPoint(layer.LocH + offset.X, layer.LocV + offset.Y);
            var scale = new LingoPoint(layer.FlipH ? -1 : 1, layer.FlipV ? -1 : 1);
            var transform = LingoTransform2D.Identity
                .Translated(-srcCenter.X, -srcCenter.Y)
                .Scaled(scale.X, scale.Y)
                .Skewed(layer.Skew)
                .Rotated(layer.Rotation)
                .Translated(pos.X, pos.Y);

            prepared.Add(new LayerInfo(
                bmp,
                srcX,
                srcY,
                srcW,
                srcH,
                destW,
                destH,
                transform,
                Math.Clamp(layer.Blend / 100f, 0f, 1f),
                layer.InkType,
                layer.BackColor));
        }

        return (offset, width, height, prepared);
    }
}

