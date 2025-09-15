using System;
using System.Collections.Generic;
using System.Linq;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Net.RNetTerminal;

internal sealed class StageView : View
{
    private const int MovieWidth = 640;
    private const int MovieHeight = 480;
    private readonly IReadOnlyList<LingoSpriteDTO> _sprites;
    private readonly Dictionary<int, LingoMemberDTO> _members = new();
    private int _frame;
    private SpriteRef? _selectedSprite;
    private static readonly Color[] OverlapColors =
    {
        Color.DarkGray,
        Color.Gray,
        Color.White,
        Color.BrightYellow
    };

    public event Action<int, int>? SpriteSelected;

    public StageView()
    {
        CanFocus = true;
        ColorScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black)
        };
        _sprites = TestMovieBuilder.BuildSprites();
        foreach (var m in TestCastBuilder.BuildCastData().SelectMany(c => c.Value))
        {
            _members[MemberKey(m)] = m;
        }
    }

    public void SetFrame(int frame)
    {
        _frame = frame;
    }

    public void RequestRedraw() => SetNeedsDisplay();

    public void SetSelectedSprite(SpriteRef? sprite)
    {
        _selectedSprite = sprite;
        SetNeedsDisplay();
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);
        var w = bounds.Width;
        var h = bounds.Height;
        Driver.SetAttribute(ColorScheme.Normal);

        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                Driver.AddRune(' ');
            }
        }

        var chars = new char[w, h];
        var colors = new Color[w, h];
        var counts = new int[w, h];

        foreach (var sprite in _sprites
                     .Where(s => s.BeginFrame <= _frame && _frame <= s.EndFrame)
                     .OrderBy(s => s.LocZ))
        {
            if (!_members.TryGetValue(sprite.MemberNum, out var member))
            {
                continue;
            }
            var x = (int)(sprite.LocH / MovieWidth * w);
            var y = (int)(sprite.LocV / MovieHeight * h);
            var sw = (int)(sprite.Width / MovieWidth * w);
            var sh = (int)(sprite.Height / MovieHeight * h);
            if (sw <= 0 || sh <= 0 || sh > 2 || sw > 10)
            {
                sw = 1;
                sh = 1;
            }
            if (member.Type == LingoMemberTypeDTO.Text)
            {
                var text = member.Name;
                var count = Math.Min(sw, text.Length);
                for (var i = 0; i < count; i++)
                {
                    var sx = x + i;
                    var sy = y;
                    if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                    {
                        continue;
                    }
                    counts[sx, sy]++;
                    chars[sx, sy] = text[i];
                    var bg = _selectedSprite.HasValue && sprite.SpriteNum == _selectedSprite.Value.SpriteNum && sprite.BeginFrame == _selectedSprite.Value.BeginFrame
                        ? Color.Blue
                        : OverlapColors[Math.Min(counts[sx, sy] - 1, OverlapColors.Length - 1)];
                    colors[sx, sy] = bg;
                }
            }
            else
            {
                var ch = member.Name.Length > 0 ? member.Name[0] : ' ';
                for (var dy = 0; dy < sh; dy++)
                {
                    for (var dx = 0; dx < sw; dx++)
                    {
                        var sx = x + dx;
                        var sy = y + dy;
                        if (sx < 0 || sx >= w || sy < 0 || sy >= h)
                        {
                            continue;
                        }
                        counts[sx, sy]++;
                        chars[sx, sy] = ch;
                        var bg = _selectedSprite.HasValue && sprite.SpriteNum == _selectedSprite.Value.SpriteNum && sprite.BeginFrame == _selectedSprite.Value.BeginFrame
                            ? Color.Blue
                            : OverlapColors[Math.Min(counts[sx, sy] - 1, OverlapColors.Length - 1)];
                        colors[sx, sy] = bg;
                    }
                }
            }
        }

        for (var y = 0; y < h; y++)
        {
            Move(0, y);
            for (var x = 0; x < w; x++)
            {
                var col = colors[x, y];
                Driver.SetAttribute(Application.Driver.MakeAttribute(Color.Black, col));
                Driver.AddRune(chars[x, y] == '\0' ? ' ' : chars[x, y]);
            }
        }
    }

    public override bool MouseEvent(MouseEvent me)
    {
        if (me.Flags.HasFlag(MouseFlags.Button1Clicked))
        {
            var w = Bounds.Width;
            var h = Bounds.Height;
            foreach (var sprite in _sprites
                         .Where(s => s.BeginFrame <= _frame && _frame <= s.EndFrame)
                         .OrderByDescending(s => s.LocZ))
            {
                var x = (int)(sprite.LocH / MovieWidth * w);
                var y = (int)(sprite.LocV / MovieHeight * h);
                var sw = (int)(sprite.Width / MovieWidth * w);
                var sh = (int)(sprite.Height / MovieHeight * h);
                if (sw <= 0 || sh <= 0 || sh > 2 || sw > 10)
                {
                    sw = 1;
                    sh = 1;
                }
                if (me.X >= x && me.X < x + sw && me.Y >= y && me.Y < y + sh)
                {
                    SpriteSelected?.Invoke(sprite.SpriteNum, sprite.BeginFrame);
                    break;
                }
            }
            return true;
        }
        return base.MouseEvent(me);
    }

    private static int MemberKey(LingoMemberDTO member)
        => (member.CastLibNum << 16) | member.NumberInCast;
}
