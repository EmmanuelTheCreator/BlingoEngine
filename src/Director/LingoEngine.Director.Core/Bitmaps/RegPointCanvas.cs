using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using LingoEngine.Bitmaps;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Bitmaps;

public class RegPointCanvas
{
    private readonly AbstGfxCanvas _canvas;
    public AbstGfxCanvas Canvas => _canvas;
    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public RegPointCanvas(ILingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("RegPointCanvas", 0, 0);
    }

    public void Draw(LingoMemberBitmap member, float scale)
    {
        _canvas.Width = member.Width * scale;
        _canvas.Height = member.Height * scale;
        _canvas.Clear(AColors.Transparent);
        float areaWidth = _canvas.Width;
        float areaHeight = _canvas.Height;
        var canvasHalf = new APoint(areaWidth / 2f, areaHeight / 2f);
        var imageHalf = new APoint(member.Width / 2f, member.Height / 2f);
        var offset = canvasHalf - imageHalf;
        var pos = (offset + member.RegPoint) * scale + canvasHalf * (1 - scale);
        _canvas.DrawLine(new APoint(pos.X, 0), new APoint(pos.X, areaHeight), AColors.Red);
        _canvas.DrawLine(new APoint(0, pos.Y), new APoint(areaWidth, pos.Y), AColors.Red);
    }
}
