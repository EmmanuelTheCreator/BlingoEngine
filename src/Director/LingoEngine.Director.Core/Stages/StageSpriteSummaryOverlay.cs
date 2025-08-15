using System;
using System.Linq;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Components;

namespace LingoEngine.Director.Core.Stages;

/// <summary>
/// Simple canvas overlay that displays information about the currently
/// selected sprite.
/// </summary>
public class StageSpriteSummaryOverlay : IHasSpriteSelectedEvent, IDisposable
{
    private readonly AbstUIGfxCanvas _canvas;
    private readonly IDirectorEventMediator _mediator;
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoTexture2D _infoIcon;
    private readonly ILingoTexture2D _behaviorIcon;
    private readonly ILingoTextureUserSubscription _infoIconSubscription;
    private readonly ILingoTextureUserSubscription _behaviorIconSubscription;

    public AbstUIGfxCanvas Canvas => _canvas;

    public bool Visible { get => Canvas.Visibility; set => Canvas.Visibility = value; }

    public StageSpriteSummaryOverlay(ILingoFrameworkFactory factory, IDirectorEventMediator mediator, IDirectorIconManager iconManager)
    {
        _mediator = mediator;
        _canvas = factory.CreateGfxCanvas("SpriteSummaryCanvas", 0, 0);
        _canvas.Visibility = false;
        _mediator.Subscribe(this);
        _iconManager = iconManager;
        _infoIcon = _iconManager.Get(DirectorIcon.Info);
        _infoIconSubscription = _infoIcon.AddUser(this);
        _behaviorIcon = _iconManager.Get(DirectorIcon.BehaviorScript);
        _behaviorIconSubscription = _behaviorIcon.AddUser(this);
    }

    /// <summary>Called when a sprite is selected.</summary>
    public void SpriteSelected(ILingoSpriteBase sprite)
    {
        if (sprite is ILingoSprite lingoSprite)
            DrawInfo(lingoSprite);
    }

    private void DrawInfo(ILingoSprite sprite)
    {
        _canvas.Clear(AColors.White);
        if (sprite is not LingoSprite2D sp || sp.Member == null)
        {
            _canvas.Visibility = false;
            return;
        }

        var member = sp.Member;
        var behaviors = string.Join(", ", sp.Behaviors.Select(b => b.Name));

        var line1 = $"{member.Name} ({sp.Member?.CastName}) {member.Type}";
        var line2 = $"Sprite {sp.SpriteNum} ({(int)sp.LocH},{(int)sp.LocV},{(int)sp.Width}x{(int)sp.Height}) {(LingoInkType)sp.Ink} {(int)(sp.Blend)}%";
        const int margin = 2;
        const int fontSize = 9;
        const int iconSize = 16;

        var lines = new[] { line1, line2 };
        var icons = new[] { _infoIcon, _infoIcon };
        if (!string.IsNullOrEmpty(behaviors))
        {
            lines = lines.Append(behaviors).ToArray();
            icons = icons.Append(_behaviorIcon).ToArray();
        }

        int height = lines.Length * iconSize + (margin * 2);
        int widthText = lines.Max(l => l.Length * 6);
        int width = Math.Max(120, widthText + iconSize + 8);

        _canvas.Width = width;
        _canvas.Height = height;
        _canvas.X = sp.Rect.Left;
        _canvas.Y = sp.Rect.Bottom;

        _canvas.DrawRect(ARect.New(0, 0, width, height), AColors.LightGray, true);
        var bugFixGodotNotUnderstandY = 12;

        for (int i = 0; i < lines.Length; i++)
        {
            int y = i * iconSize + margin;
            _canvas.DrawPicture(icons[i], iconSize, iconSize, new APoint(2, y));
            _canvas.DrawText(new APoint(iconSize + 4, y + bugFixGodotNotUnderstandY), lines[i], null, AColors.Black, fontSize);
        }

        _canvas.Visibility = true;
    }

    public void Dispose()
    {
        _mediator.Unsubscribe(this);
        _infoIconSubscription.Release();
        _behaviorIconSubscription.Release();
        _canvas.Dispose();
    }
}
