using Godot;
using System.Collections.Generic;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotTransitionGridChannel : DirGodotTopGridChannel<DirGodotTransitionSprite>
{
    public DirGodotTransitionGridChannel(DirScoreGfxValues gfxValues)
        : base(gfxValues)
    {
    }

    protected override IEnumerable<DirGodotTransitionSprite> BuildSprites(LingoMovie movie)
    {
        foreach (var k in movie.GetTransitionKeyframes())
            yield return new DirGodotTransitionSprite(k);
    }

    protected override void MoveSprite(DirGodotTransitionSprite sprite, int oldFrame, int newFrame)
    {
        _movie!.MoveTransitionKeyframe(oldFrame, newFrame);
    }

    protected override void SubscribeMovie(LingoMovie movie)
    {
        movie.TransitionKeyframesChanged += OnKeyframesChanged;
    }

    protected override void UnsubscribeMovie(LingoMovie movie)
    {
        movie.TransitionKeyframesChanged -= OnKeyframesChanged;
    }

    private void OnKeyframesChanged()
    {
        if (_movie == null) return;
        _sprites.Clear();
        foreach (var k in _movie.GetTransitionKeyframes())
            _sprites.Add(new DirGodotTransitionSprite(k));
        MarkDirty();
    }

    protected override void OnDoubleClick(int frame, DirGodotTransitionSprite? sprite)
    {
        _movie!.AddTransitionKeyFrame(frame, 0);
        MarkDirty();
    }
}
