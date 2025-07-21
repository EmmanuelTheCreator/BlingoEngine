using System;
using System.Linq;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Bitmaps;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Simple canvas overlay that displays information about the currently
/// selected sprite.
/// </summary>
public class StageSpriteSummaryOverlay : IHasSpriteSelectedEvent, IDisposable
{
    private readonly LingoGfxCanvas _canvas;
    private readonly IDirectorEventMediator _mediator;
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoImageTexture _infoIcon;
    private readonly ILingoImageTexture _behaviorIcon;

    public LingoGfxCanvas Canvas => _canvas;

    public StageSpriteSummaryOverlay(ILingoFrameworkFactory factory, IDirectorEventMediator mediator, IDirectorIconManager iconManager)
    {
        _mediator = mediator;
        _canvas = factory.CreateGfxCanvas("SpriteSummaryCanvas", 0, 0);
        _canvas.Visibility = false;
        _mediator.Subscribe(this);
        _iconManager = iconManager;
        _infoIcon = _iconManager.Get(DirectorIcon.Info);
        _behaviorIcon = _iconManager.Get(DirectorIcon.BehaviorScript);
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

        const int fontSize = 10;
        const int iconSize = 16;
        var lines = new[] { line1, line2 };
        var icons = new[] { _infoIcon, _infoIcon };
        if (!string.IsNullOrEmpty(behaviors))
        {
            lines = lines.Append(behaviors).ToArray();
            icons = icons.Append(_behaviorIcon).ToArray();
        }

        int height = lines.Length * (fontSize + 2) + 4;
        int widthText = lines.Max(l => l.Length * 6);
        int width = Math.Max(120, widthText + iconSize + 8);

        _canvas.Width = width;
        _canvas.Height = height;
        _canvas.X = sp.Rect.Left;
        _canvas.Y = sp.Rect.Bottom;

        _canvas.DrawRect(LingoRect.New(0, 0, width, height), LingoColorList.Gray, true);

        for (int i = 0; i < lines.Length; i++)
        {
            int y = 2 + i * (fontSize + 2);
            _canvas.DrawPicture(icons[i], iconSize, iconSize, new LingoPoint(2, y));
            _canvas.DrawText(new LingoPoint(iconSize + 4, y), lines[i], null, LingoColorList.White, fontSize);
        }

        _canvas.Visibility = true;
    }

    public void Dispose()
    {
        _mediator.Unsubscribe(this);
        _canvas.Dispose();
    }
}
