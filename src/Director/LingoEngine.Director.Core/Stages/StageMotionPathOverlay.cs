using System;
using LingoEngine.Animations;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Overlay that draws the motion path for a sprite.
/// </summary>
public class StageMotionPathOverlay : IDisposable
{
    private readonly LingoGfxCanvas _canvas;

    public LingoGfxCanvas Canvas => _canvas;

    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public StageMotionPathOverlay(ILingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("MotionPathCanvas", 3000, 2000);
        _canvas.Visibility = false;
    }

    public void Draw(LingoSpriteMotionPath? path)
    {
        _canvas.Clear(LingoColorList.Transparent);
        if (path == null || path.Frames.Count == 0)
        {
            _canvas.Visibility = false;
            return;
        }

        for (int i = 1; i < path.Frames.Count; i++)
        {
            var prev = path.Frames[i - 1].Position;
            var curr = path.Frames[i].Position;
            _canvas.DrawLine(prev, curr, LingoColorList.Yellow);
        }

        var first = path.Frames[0];
        var last = path.Frames[^1];
        DrawCircle(first.Position, LingoColorList.Green, true, 2.5f);
        DrawCircle(last.Position, LingoColorList.Red, true, 2.5f);

        for (int i = 1; i < path.Frames.Count - 1; i++)
        {
            var f = path.Frames[i];
            if (f.IsKeyFrame)
                DrawCircle(f.Position, LingoColorList.Yellow, true, 2.5f);
            else
                DrawCircle(f.Position, LingoColorList.Yellow, false, 1.5f);
        }

        _canvas.Visibility = true;
    }

    private void DrawCircle(LingoPoint position, LingoColor fillColor, bool keyframe, float radius)
    {
        _canvas.DrawCircle(position, radius, fillColor, true);
        if (keyframe)
            _canvas.DrawCircle(position, radius, LingoColorList.Black, false, 1);
    }

    public void Dispose()
    {
        _canvas.Dispose();
    }
}
