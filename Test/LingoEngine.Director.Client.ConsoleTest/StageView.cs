using System.Collections.Generic;
using System.Linq;
using LingoEngine.IO.Data.DTO;
using Terminal.Gui;

namespace LingoEngine.Director.Client.ConsoleTest;

internal sealed class StageView : View
{
    private const int MovieWidth = 640;
    private const int MovieHeight = 480;
    private readonly IReadOnlyList<LingoSpriteDTO> _sprites;
    private int _frame;

    public StageView()
    {
        CanFocus = false;
        _sprites = TestMovieBuilder.BuildSprites();
    }

    public void SetFrame(int frame)
    {
        _frame = frame;
        SetNeedsDisplay();
    }

    public void Attach(ScoreView score)
    {
        score.SpriteChanged += OnSpriteChanged;
    }

    private void OnSpriteChanged()
    {
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

        foreach (var sprite in _sprites.Where(s => s.BeginFrame <= _frame && _frame <= s.EndFrame))
        {
            var ch = sprite.Name.Length > 0 ? sprite.Name[0] : '?';
            var x = (int)(sprite.LocH / MovieWidth * w);
            var y = (int)(sprite.LocV / MovieHeight * h);
            if (x >= 0 && x < w && y >= 0 && y < h)
            {
                Move(x, y);
                Driver.AddRune(ch);
            }
        }
    }
}
