using System;
using System.Linq;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Simple canvas overlay that displays information about the currently
/// selected sprite.
/// </summary>
public class StageSpriteSummaryOverlay : IHasSpriteSelectedEvent, IDisposable
{
    private readonly LingoGfxCanvas _canvas;
    private readonly IDirectorEventMediator _mediator;

    public LingoGfxCanvas Canvas => _canvas;

    public StageSpriteSummaryOverlay(ILingoFrameworkFactory factory, IDirectorEventMediator mediator)
    {
        _mediator = mediator;
        _canvas = factory.CreateGfxCanvas("SpriteSummaryCanvas", 0, 0);
        _canvas.Visibility = false;
        _mediator.Subscribe(this);
    }

    /// <summary>Called when a sprite is selected.</summary>
    public void SpriteSelected(ILingoSprite sprite)
    {
        DrawInfo(sprite);
    }

    private void DrawInfo(ILingoSprite sprite)
    {
        _canvas.Clear(LingoColorList.White);
        if (sprite is not LingoSprite sp || sp.Member == null)
        {
            _canvas.Visibility = false;
            return;
        }

        var member = sp.Member;
        var behaviors = string.Join(", ", sp.Behaviors.Select(b => b.Name));

        var line1 = $"{member.Name} ({sp.Cast?.Name}) {member.Type}";
        var line2 = $"Sprite {sp.SpriteNum} ({(int)sp.LocH},{(int)sp.LocV},{(int)sp.Width}x{(int)sp.Height}) {(LingoInkType)sp.Ink} {(int)(sp.Blend * 100)}%";
        string text = line1 + "\n" + line2;
        if (!string.IsNullOrEmpty(behaviors))
            text += "\n" + behaviors;

        const int fontSize = 10;
        var lines = text.Split('\n');
        int height = lines.Length * (fontSize + 2) + 4;
        int width = Math.Max(120, lines.Max(l => l.Length) * 6 + 8);

        _canvas.Width = width;
        _canvas.Height = height;
        _canvas.X = sp.Rect.Left;
        _canvas.Y = sp.Rect.Bottom;

        _canvas.DrawRect(LingoRect.New(0, 0, width, height), LingoColorList.Gray, true);
        _canvas.DrawText(new LingoPoint(4, 2), text, null, LingoColorList.White, fontSize);
        _canvas.Visibility = true;
    }

    public void Dispose()
    {
        _mediator.Unsubscribe(this);
        _canvas.Dispose();
    }
}
