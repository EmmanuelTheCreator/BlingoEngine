using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotColorPaletteGridChannel : DirGodotTopGridChannel<DirGodotColorPaletteSprite>
{
    public DirGodotColorPaletteGridChannel(DirScoreGfxValues gfxValues)
        : base(gfxValues)
    {
    }

    protected override IEnumerable<DirGodotColorPaletteSprite> BuildSprites(LingoMovie movie)
    {
        foreach (var k in movie.GetColorPaletteKeyframes())
            yield return new DirGodotColorPaletteSprite(k);
    }

    protected override void MoveSprite(DirGodotColorPaletteSprite sprite, int oldFrame, int newFrame)
    {
        _movie!.MoveColorPaletteKeyframe(oldFrame, newFrame);
    }

    protected override void SubscribeMovie(LingoMovie movie)
    {
        movie.ColorPaletteKeyframesChanged += OnKeyframesChanged;
    }

    protected override void UnsubscribeMovie(LingoMovie movie)
    {
        movie.ColorPaletteKeyframesChanged -= OnKeyframesChanged;
    }

    private void OnKeyframesChanged()
    {
        if (_movie == null) return;
        _sprites.Clear();
        foreach (var k in _movie.GetColorPaletteKeyframes())
            _sprites.Add(new DirGodotColorPaletteSprite(k));
        MarkDirty();
    }

    protected override void OnDoubleClick(int frame, DirGodotColorPaletteSprite? sprite)
    {
        _movie!.AddColorPaletteKeyFrame(frame, 0);
        MarkDirty();
    }
}
