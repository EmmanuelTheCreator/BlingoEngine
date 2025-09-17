using System;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.FrameworkCommunication;

namespace BlingoEngine.Director.Core.Stages;

/// <summary>
/// Overlay that draws the motion path for a sprite.
/// </summary>
public class StageMotionPathOverlay : IDisposable
{
    private readonly AbstGfxCanvas _canvas;

    public AbstGfxCanvas Canvas => _canvas;

    public bool Visible { get => _canvas.Visibility; set => _canvas.Visibility = value; }

    public StageMotionPathOverlay(IBlingoFrameworkFactory factory)
    {
        _canvas = factory.CreateGfxCanvas("MotionPathCanvas", 3000, 2000);
        _canvas.Visibility = false;
    }

    public void Draw(BlingoSpriteMotionPath? path)
    {
        _canvas.Clear(AColors.Transparent);
        if (path == null || path.Frames.Count == 0)
        {
            _canvas.Visibility = false;
            return;
        }

        for (int i = 1; i < path.Frames.Count; i++)
        {
            var prev = path.Frames[i - 1].Position;
            var curr = path.Frames[i].Position;
            _canvas.DrawLine(prev, curr, AColors.Yellow);
        }

        var first = path.Frames[0];
        var last = path.Frames[^1];
        DrawCircle(first.Position, AColors.Green, true, 2.5f);
        DrawCircle(last.Position, AColors.Red, true, 2.5f);

        for (int i = 1; i < path.Frames.Count - 1; i++)
        {
            var f = path.Frames[i];
            if (f.IsKeyFrame)
                DrawCircle(f.Position, AColors.Yellow, true, 2.5f);
            else
                DrawCircle(f.Position, AColors.Yellow, false, 1.5f);
        }

        _canvas.Visibility = true;
    }

    private void DrawCircle(APoint position, AColor fillColor, bool keyframe, float radius)
    {
        _canvas.DrawCircle(position, radius, fillColor, true);
        if (keyframe)
            _canvas.DrawCircle(position, radius, AColors.Black, false, 1);
    }

    public void Dispose()
    {
        _canvas.Dispose();
    }
}

