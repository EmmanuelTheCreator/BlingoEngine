using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using BlingoEngine.FrameworkCommunication;

namespace BlingoEngine.Director.Core.Bitmaps;

public class PaintCanvasV2
{
    private readonly AbstGfxCanvas _canvas;
    public AbstGfxCanvas Canvas => _canvas;

    public PaintCanvasV2(IBlingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("PaintCanvas", 0, 0);
    }

    public void Draw(PicturePainterV2 painter, float scale)
    {
        _canvas.Width = painter.Width * scale;
        _canvas.Height = painter.Height * scale;
        foreach (var (pos, texture) in painter.GetDirtyTiles())
        {
            var origin = painter.Offset + pos;
            int size = (int)(painter.TileSize * scale);
            var dest = new APoint(origin.X * scale, origin.Y * scale);
            _canvas.DrawPicture(texture, size, size, dest);
        }
    }

    public void DrawAll(PicturePainterV2 painter, float scale)
    {
        _canvas.Width = painter.Width * scale;
        _canvas.Height = painter.Height * scale;
        foreach (var (pos, texture) in painter.GetTiles())
        {
            var origin = painter.Offset + pos;
            int size = (int)(painter.TileSize * scale);
            var dest = new APoint(origin.X * scale, origin.Y * scale);
            _canvas.DrawPicture(texture, size, size, dest);
        }
    }
}

