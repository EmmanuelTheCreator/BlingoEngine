using LingoEngine.Bitmaps;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Bitmaps;

public class RegPointCanvas
{
    private readonly LingoGfxCanvas _canvas;
    public LingoGfxCanvas Canvas => _canvas;
    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public RegPointCanvas(ILingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("RegPointCanvas", 0, 0);
    }

    public void Draw(LingoMemberBitmap member, float scale)
    {
        _canvas.Width = member.Width * scale;
        _canvas.Height = member.Height * scale;
        _canvas.Clear(LingoColorList.Transparent);
        float areaWidth = _canvas.Width;
        float areaHeight = _canvas.Height;
        var canvasHalf = new LingoPoint(areaWidth / 2f, areaHeight / 2f);
        var imageHalf = new LingoPoint(member.Width / 2f, member.Height / 2f);
        var offset = canvasHalf - imageHalf;
        var pos = (offset + member.RegPoint) * scale + canvasHalf * (1 - scale);
        _canvas.DrawLine(new LingoPoint(pos.X, 0), new LingoPoint(pos.X, areaHeight), LingoColorList.Red);
        _canvas.DrawLine(new LingoPoint(0, pos.Y), new LingoPoint(areaWidth, pos.Y), LingoColorList.Red);
    }
}
